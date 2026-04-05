"""
optimizer.py — Dual-mode covering solver

Strategy:
  - C(n, k) <= EXACT_THRESHOLD  →  Exact backtracking + Branch & Bound
                                    Guarantees the provably MINIMUM number of groups.
  - C(n, k) >  EXACT_THRESHOLD  →  Greedy + random restarts (near-optimal, fast)
  - If exact backtracking exceeds TIMEOUT_SECONDS, automatically falls back to greedy.

Returns (groups, method) where method is "exact" or "greedy".
"""

import itertools
import random
import threading
from math import comb
import numpy as np

EXACT_THRESHOLD = 84    # C(9,6)=84 is the practical limit for fast exact solve
TIMEOUT_SECONDS = 3     # fall back to greedy if exact takes longer than this


# ─────────────────────────────────────────────────────────────────────────────
# Shared helpers
# ─────────────────────────────────────────────────────────────────────────────

def _build_coverage_index(all_k_groups, j_groups, s):
    """
    For each k-group, pre-compute the set of j-group indices it covers.
    A k-group covers a j-group if |intersection| >= s.
    Returns: list of frozensets (one per k-group)
    """
    coverage = []
    for kg in all_k_groups:
        kg_set = set(kg)
        covered = frozenset(
            idx for idx, jg in enumerate(j_groups)
            if len(kg_set & set(jg)) >= s
        )
        coverage.append(covered)
    return coverage


def _deduplicate(all_k_groups, coverage):
    """
    Remove k-groups with identical coverage fingerprints — they are
    interchangeable, so keeping only one shrinks the search space.
    Returns pruned (all_k_groups, coverage).
    """
    seen = {}
    pruned_groups, pruned_cov = [], []
    for kg, cov in zip(all_k_groups, coverage):
        if cov not in seen:
            seen[cov] = True
            pruned_groups.append(kg)
            pruned_cov.append(cov)
    return pruned_groups, pruned_cov


# ─────────────────────────────────────────────────────────────────────────────
# MODE 1 — Exact backtracking with Branch & Bound
# ─────────────────────────────────────────────────────────────────────────────

def _exact_backtrack(samples, k, j, s, min_coverage):
    """
    Exact Branch-and-Bound set cover with timeout.
    Returns (groups, timed_out).
    """
    all_k_groups = list(itertools.combinations(samples, k))
    j_groups     = list(itertools.combinations(samples, j))
    n_j          = len(j_groups)

    coverage = _build_coverage_index(all_k_groups, j_groups, s)
    all_k_groups, coverage = _deduplicate(all_k_groups, coverage)

    order = sorted(range(len(all_k_groups)),
                   key=lambda i: len(coverage[i]), reverse=True)
    all_k_groups = [all_k_groups[i] for i in order]
    coverage     = [coverage[i]     for i in order]

    greedy_sol = _greedy_once(samples, k, j, s, min_coverage,
                               list(itertools.combinations(samples, k)))
    best       = [greedy_sol]
    timed_out  = [False]
    stop_event = threading.Event()

    cover_count = [0] * n_j

    def covered_enough():
        return all(c >= min_coverage for c in cover_count)

    def backtrack(start, selected):
        if stop_event.is_set():
            return
        if covered_enough():
            if len(selected) < len(best[0]):
                best[0] = list(selected)
            return
        remaining        = len(all_k_groups) - start
        uncovered_count  = sum(1 for c in cover_count if c < min_coverage)
        if uncovered_count == 0:
            if len(selected) < len(best[0]):
                best[0] = list(selected)
            return
        if remaining == 0:
            return
        max_single = max((len(coverage[i]) for i in range(start, len(all_k_groups))),
                         default=0)
        if max_single == 0:
            return
        import math
        lb = math.ceil(uncovered_count / max_single)
        if len(selected) + lb >= len(best[0]):
            return

        for i in range(start, len(all_k_groups)):
            if stop_event.is_set():
                return
            kg  = all_k_groups[i]
            cov = coverage[i]
            selected.append(kg)
            for idx in cov:
                cover_count[idx] += 1
            backtrack(i + 1, selected)
            selected.pop()
            for idx in cov:
                cover_count[idx] -= 1

    def run():
        backtrack(0, [])

    t = threading.Thread(target=run)
    t.start()
    t.join(timeout=TIMEOUT_SECONDS)
    if t.is_alive():
        stop_event.set()
        timed_out[0] = True
        t.join()

    return best[0], timed_out[0]


# ─────────────────────────────────────────────────────────────────────────────
# MODE 2 — Greedy + random restarts
# ─────────────────────────────────────────────────────────────────────────────

def _greedy_once(samples, k, j, s, min_coverage, all_k_groups_input):
    """Numpy-accelerated greedy set cover."""
    all_k_groups = list(all_k_groups_input)
    j_groups     = list(itertools.combinations(samples, j))
    n_j          = len(j_groups)
    n_kg         = len(all_k_groups)

    # Build coverage matrix: cover[i, t] = 1 if k-group i covers j-group t
    elem_to_idx = {e: i for i, e in enumerate(samples)}
    j_sets = [set(jg) for jg in j_groups]

    cover = np.zeros((n_kg, n_j), dtype=np.int8)
    for i, kg in enumerate(all_k_groups):
        kg_set = set(kg)
        for t, jg_set in enumerate(j_sets):
            if len(kg_set & jg_set) >= s:
                cover[i, t] = 1

    cover_count = np.zeros(n_j, dtype=np.int32)
    selected    = []
    available   = list(range(n_kg))

    while True:
        uncovered_mask = cover_count < min_coverage
        if not uncovered_mask.any():
            break
        if not available:
            raise RuntimeError(
                f"Cannot satisfy coverage={min_coverage} with "
                f"k={k}, j={j}, s={s} on {len(samples)} samples."
            )

        # Gain = number of newly covered j-subsets each candidate adds
        gains = cover[available][:, uncovered_mask].sum(axis=1)
        best_local = int(np.argmax(gains))
        best_idx   = available[best_local]

        selected.append(all_k_groups[best_idx])
        cover_count += cover[best_idx]
        available.pop(best_local)

    return selected


def _greedy_with_restarts(samples, k, j, s, min_coverage, restarts=10):
    all_k_groups = list(itertools.combinations(samples, k))
    best = None
    for attempt in range(restarts):
        shuffled = list(all_k_groups)
        if attempt > 0:
            random.shuffle(shuffled)
        try:
            result = _greedy_once(samples, k, j, s, min_coverage, shuffled)
            if best is None or len(result) < len(best):
                best = result
        except RuntimeError:
            if best is None and attempt == restarts - 1:
                raise
    return best


# ─────────────────────────────────────────────────────────────────────────────
# Public API
# ─────────────────────────────────────────────────────────────────────────────

def find_minimal_valid_k_groups(samples, k, j, s,
                                 min_coverage=1, restarts=10):
    """
    Find the minimal set of k-groups covering every j-subset of `samples`
    at least `min_coverage` times with >= s overlap.

    Returns
    -------
    (groups, method)
        groups : list of tuples
        method : "exact" | "greedy"
    """
    n_k_groups = comb(len(samples), k)

    if n_k_groups <= EXACT_THRESHOLD and min_coverage == 1:
        groups, timed_out = _exact_backtrack(samples, k, j, s, min_coverage)
        if not timed_out:
            return groups, "exact"
        # Timed out — fall through to greedy below

    groups = _greedy_with_restarts(samples, k, j, s, min_coverage, restarts=5)
    return groups, "greedy"

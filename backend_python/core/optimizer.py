"""
optimizer.py — Dual-mode covering solver

Strategy (same as industry best practice):
  - C(n, k) <= EXACT_THRESHOLD  →  Exact backtracking + Branch & Bound
                                    Guarantees the provably MINIMUM number of groups.
  - C(n, k) >  EXACT_THRESHOLD  →  Greedy + random restarts (near-optimal, fast)

Returns (groups, method) where method is "exact" or "greedy".
"""

import itertools
import random
from math import comb

EXACT_THRESHOLD = 200   # switch to exact when total k-groups <= this


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
    Exact Branch-and-Bound set cover.
    Guarantees the minimum number of k-groups needed.
    """
    all_k_groups = list(itertools.combinations(samples, k))
    j_groups     = list(itertools.combinations(samples, j))
    n_j          = len(j_groups)

    coverage = _build_coverage_index(all_k_groups, j_groups, s)
    all_k_groups, coverage = _deduplicate(all_k_groups, coverage)

    # Sort by coverage size descending (most powerful first — better pruning)
    order = sorted(range(len(all_k_groups)),
                   key=lambda i: len(coverage[i]), reverse=True)
    all_k_groups = [all_k_groups[i] for i in order]
    coverage     = [coverage[i]     for i in order]

    # Initial upper bound from greedy
    greedy_sol = _greedy_once(samples, k, j, s, min_coverage,
                               list(itertools.combinations(samples, k)))
    best = [greedy_sol]          # best[0] holds current best solution

    # cover_count[idx] = how many selected groups cover j-group idx
    cover_count = [0] * n_j

    def covered_enough():
        return all(c >= min_coverage for c in cover_count)

    def backtrack(start, selected):
        if covered_enough():
            if len(selected) < len(best[0]):
                best[0] = list(selected)
            return

        remaining = len(all_k_groups) - start
        uncovered_count = sum(1 for c in cover_count if c < min_coverage)
        if uncovered_count == 0:
            if len(selected) < len(best[0]):
                best[0] = list(selected)
            return

        # Lower-bound pruning: even if every remaining group covers max possible,
        # can we beat the current best?
        if remaining == 0:
            return
        max_single = max((len(coverage[i]) for i in range(start, len(all_k_groups))),
                         default=0)
        if max_single == 0:
            return
        import math
        lb = math.ceil(uncovered_count / max_single)
        if len(selected) + lb >= len(best[0]):
            return   # cannot improve — prune

        for i in range(start, len(all_k_groups)):
            kg  = all_k_groups[i]
            cov = coverage[i]

            # Apply
            selected.append(kg)
            for idx in cov:
                cover_count[idx] += 1

            backtrack(i + 1, selected)

            # Undo
            selected.pop()
            for idx in cov:
                cover_count[idx] -= 1

    backtrack(0, [])
    return best[0]


# ─────────────────────────────────────────────────────────────────────────────
# MODE 2 — Greedy + random restarts
# ─────────────────────────────────────────────────────────────────────────────

def _greedy_once(samples, k, j, s, min_coverage, all_k_groups_input):
    all_k_groups = list(all_k_groups_input)
    j_groups     = list(itertools.combinations(samples, j))
    cover_count  = {jg: 0 for jg in j_groups}
    selected     = []

    while any(cover_count[jg] < min_coverage for jg in j_groups):
        uncovered = [jg for jg in j_groups if cover_count[jg] < min_coverage]

        freq = {}
        for jg in uncovered:
            for elem in jg:
                freq[elem] = freq.get(elem, 0) + 1

        best_group, best_gain = None, -1
        for kg in all_k_groups:
            kg_set = set(kg)
            gain   = sum(1 for jg in uncovered if len(kg_set & set(jg)) >= s)
            if gain > best_gain:
                best_gain, best_group = gain, kg
            elif gain == best_gain and best_group is not None:
                if sum(freq.get(e, 0) for e in kg) > sum(freq.get(e, 0) for e in best_group):
                    best_group = kg

        if best_group is None or best_gain == 0:
            raise RuntimeError(
                f"Cannot satisfy coverage={min_coverage} with "
                f"k={k}, j={j}, s={s} on {len(samples)} samples."
            )

        selected.append(best_group)
        all_k_groups.remove(best_group)
        kg_set = set(best_group)
        for jg in j_groups:
            if cover_count[jg] < min_coverage and len(kg_set & set(jg)) >= s:
                cover_count[jg] += 1

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
        # Use provably optimal backtracking
        groups = _exact_backtrack(samples, k, j, s, min_coverage)
        return groups, "exact"
    else:
        # Use greedy approximation with random restarts
        groups = _greedy_with_restarts(samples, k, j, s, min_coverage, restarts)
        return groups, "greedy"

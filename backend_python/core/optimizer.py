import itertools
import random


def _greedy_once(samples, k, j, s, min_coverage, all_k_groups_input):
    all_k_groups = list(all_k_groups_input)
    j_groups = list(itertools.combinations(samples, j))
    cover_count = {jg: 0 for jg in j_groups}
    selected = []

    while any(cover_count[jg] < min_coverage for jg in j_groups):
        best_group = None
        best_gain = -1
        uncovered = [jg for jg in j_groups if cover_count[jg] < min_coverage]

        freq = {}
        for jg in uncovered:
            for elem in jg:
                freq[elem] = freq.get(elem, 0) + 1

        for kg in all_k_groups:
            kg_set = set(kg)
            gain = sum(1 for jg in uncovered if len(kg_set & set(jg)) >= s)
            if gain > best_gain:
                best_gain = gain
                best_group = kg
            elif gain == best_gain and best_group is not None:
                score_new  = sum(freq.get(e, 0) for e in kg)
                score_best = sum(freq.get(e, 0) for e in best_group)
                if score_new > score_best:
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


def find_minimal_valid_k_groups(samples, k, j, s, min_coverage=1, restarts=10):
    """
    Greedy + random-restart to find the minimal set of k-groups
    covering every j-subset at least min_coverage times with >= s overlap.
    """
    all_k_groups = list(itertools.combinations(samples, k))
    best_result = None

    for attempt in range(restarts):
        shuffled = list(all_k_groups)
        if attempt > 0:
            random.shuffle(shuffled)
        try:
            result = _greedy_once(samples, k, j, s, min_coverage, shuffled)
            if best_result is None or len(result) < len(best_result):
                best_result = result
        except RuntimeError:
            if best_result is None and attempt == restarts - 1:
                raise

    return best_result

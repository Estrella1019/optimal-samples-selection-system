import itertools

def satisfies_constraints(k_groups, n_samples, j, s):
    """
    Check if every j-subset of n_samples is covered by at least one
    k-group with >= s overlapping elements.
    """
    j_groups = list(itertools.combinations(n_samples, j))

    for j_group in j_groups:
        j_set = set(j_group)
        covered = any(len(j_set & set(k)) >= s for k in k_groups)
        if not covered:
            return False
    return True

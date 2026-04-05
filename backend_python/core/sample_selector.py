import random
import itertools

def select_n_samples(m, n, manual_input=None):
    """
    Select n samples from m. If manual_input is provided, use it.
    Returns a sorted list of integers.
    """
    full_set = list(range(1, m + 1))

    if manual_input:
        if len(manual_input) != n:
            raise ValueError("Manual input must contain exactly n values.")
        if any(x not in full_set for x in manual_input):
            raise ValueError("Manual input contains invalid numbers.")
        return sorted(manual_input)

    return sorted(random.sample(full_set, n))


def generate_k_groups(n_samples, k):
    """
    Generate all combinations of k elements from the selected n_samples.
    Returns a list of tuples.
    """
    return list(itertools.combinations(n_samples, k))

import os
import itertools
import sys

from core.sample_selector import select_n_samples
from core.optimizer       import find_minimal_valid_k_groups
from core.constraints     import satisfies_constraints

# params.txt is written by the C# frontend next to this script's project root
PARAMS_FILE = os.path.abspath(
    os.path.join(
        os.path.dirname(__file__),
        "..", "frontend_csharp", "params.txt"
    )
)


def parse_params(filepath=PARAMS_FILE):
    """
    Parses m, n, k, j, s, manual_input, min_coverage from params.txt.
    Defaults: manual_input=None, min_coverage=1.
    """
    params = {"manual_input": None, "min_coverage": 1}
    with open(filepath, "r", encoding="utf-8") as f:
        for raw in f:
            line = raw.strip()
            if not line or "=" not in line:
                continue
            key, val = line.split("=", 1)
            key = key.strip()
            val = val.strip()
            if val == "":
                continue

            if key == "manual_input":
                nums = [int(x) for x in val.split(",") if x.strip()]
                params[key] = nums if nums else None
            elif key == "min_coverage":
                params[key] = max(1, int(val))
            else:
                params[key] = int(val)
    return params


def _next_run_id(out_dir, m, n, k, j, s):
    """
    Scan the results directory and return the next run_id for this parameter set.
    Filename format: m-n-k-j-s-{run_id}-{count}.txt
    """
    prefix = f"{m}-{n}-{k}-{j}-{s}-"
    if not os.path.isdir(out_dir):
        return 1
    existing = [
        f for f in os.listdir(out_dir)
        if f.startswith(prefix) and f.endswith(".txt")
    ]
    if not existing:
        return 1
    ids = []
    for fname in existing:
        parts = fname[len(prefix):].replace(".txt", "").split("-")
        try:
            ids.append(int(parts[0]))
        except (ValueError, IndexError):
            pass
    return max(ids, default=0) + 1


def save_results(selected, groups, params, run_id):
    """
    Writes:
      - a uniquely named result file: frontend_csharp/bin/Debug/results/m-n-k-j-s-{run_id}-{count}.txt
      - the UI's live file:           frontend_csharp/bin/Debug/results.txt
    """
    base = os.path.abspath(
        os.path.join(os.path.dirname(__file__), "..", "frontend_csharp", "bin", "Debug")
    )
    out_dir = os.path.join(base, "results")
    os.makedirs(out_dir, exist_ok=True)

    m, n, k, j, s = params["m"], params["n"], params["k"], params["j"], params["s"]
    count = len(groups)
    fname = f"{m}-{n}-{k}-{j}-{s}-{run_id}-{count}.txt"
    full_path = os.path.join(out_dir, fname)
    ui_path   = os.path.join(base, "results.txt")

    # Verify coverage (sanity check)
    verified = satisfies_constraints(groups, selected, j, s)

    lines = []
    lines.append("=" * 50)
    lines.append("  Optimal Samples Selection — Results")
    lines.append("=" * 50)
    lines.append(f"Selected Samples ({n} from {m}):")
    lines.append(f"  {selected}")
    lines.append("")
    lines.append(f"Parameters: m={m}, n={n}, k={k}, j={j}, s={s}, "
                 f"min_coverage={params['min_coverage']}")
    lines.append(f"Run ID: {run_id}")
    lines.append(f"Coverage verified: {'YES' if verified else 'NO (bug!)'}")
    lines.append("")
    lines.append(f"Valid k-groups ({count} total):")
    for idx, grp in enumerate(groups, 1):
        lines.append(f"  {idx:3d}. {list(grp)}")
    lines.append("")
    lines.append(f"Total groups: {count}")
    lines.append(f"Output saved to: {fname}")
    lines.append("=" * 50)

    content = "\n".join(lines)

    for path in (full_path, ui_path):
        with open(path, "w", encoding="utf-8") as f:
            f.write(content)

    # stdout is captured by the C# frontend
    print(content)

    return full_path


def main():
    try:
        params   = parse_params()
        selected = select_n_samples(
                       params["m"],
                       params["n"],
                       params.get("manual_input")
                   )

        groups = find_minimal_valid_k_groups(
                     selected,
                     params["k"],
                     params["j"],
                     params["s"],
                     min_coverage=params["min_coverage"],
                     restarts=8
                 )

        # Auto-increment run_id
        base    = os.path.abspath(
                      os.path.join(os.path.dirname(__file__),
                                   "..", "frontend_csharp", "bin", "Debug")
                  )
        out_dir = os.path.join(base, "results")
        run_id  = _next_run_id(
                      out_dir,
                      params["m"], params["n"], params["k"],
                      params["j"], params["s"]
                  )

        save_results(selected, groups, params, run_id)

    except Exception as e:
        err = f"Error: {e}"
        base    = os.path.abspath(
                      os.path.join(os.path.dirname(__file__),
                                   "..", "frontend_csharp", "bin", "Debug")
                  )
        ui_path = os.path.join(base, "results.txt")
        os.makedirs(os.path.dirname(ui_path), exist_ok=True)
        for p in ("results.txt", ui_path):
            try:
                with open(p, "w", encoding="utf-8") as f:
                    f.write(err)
            except Exception:
                pass
        print(err)
        sys.exit(1)


if __name__ == "__main__":
    main()

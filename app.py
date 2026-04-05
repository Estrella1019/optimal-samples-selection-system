"""
Optimal Samples Selection System — Flask Web App
Run:  python app.py
Open: http://localhost:5000  (or your LAN IP for mobile)
"""

import os, sys, json, random, itertools

from flask import Flask, render_template, request, jsonify, redirect, url_for

# ── Make sure backend_python is importable ──────────────────────────────────
BASE_DIR = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.join(BASE_DIR, "backend_python"))

from core.sample_selector import select_n_samples
from core.optimizer       import find_minimal_valid_k_groups
from core.constraints     import satisfies_constraints

RESULTS_DIR = os.path.join(BASE_DIR, "results")
os.makedirs(RESULTS_DIR, exist_ok=True)

app = Flask(__name__)


# ────────────────────────────────────────────────────────────────────────────
# Helpers
# ────────────────────────────────────────────────────────────────────────────

def _next_run_id(m, n, k, j, s):
    prefix = f"{m}-{n}-{k}-{j}-{s}-"
    ids = []
    for fname in os.listdir(RESULTS_DIR):
        if fname.startswith(prefix) and fname.endswith(".txt"):
            parts = fname[len(prefix):].replace(".txt", "").split("-")
            try:
                ids.append(int(parts[0]))
            except (ValueError, IndexError):
                pass
    return max(ids, default=0) + 1


def _validate(m, n, k, j, s):
    errors = []
    if not (45 <= m <= 54): errors.append("m must be between 45 and 54")
    if not (7  <= n <= 25): errors.append("n must be between 7 and 25")
    if not (4  <= k <= 7):  errors.append("k must be between 4 and 7")
    if not (4  <= j <= 7):  errors.append("j must be between 4 and 7")
    if not (3  <= s <= 7):  errors.append("s must be between 3 and 7")
    if n > m: errors.append("n cannot be greater than m")
    if k > n: errors.append("k cannot be greater than n")
    if j > n: errors.append("j cannot be greater than n")
    if s > j: errors.append("s cannot be greater than j")
    if s > k: errors.append("s cannot be greater than k")
    return errors


def _run_algorithm(m, n, k, j, s, min_coverage, manual_input):
    selected        = select_n_samples(m, n, manual_input if manual_input else None)
    groups, method  = find_minimal_valid_k_groups(
                          selected, k, j, s,
                          min_coverage=min_coverage,
                          restarts=10
                      )
    verified = satisfies_constraints(groups, selected, j, s)
    return selected, groups, verified, method


def _save_result(m, n, k, j, s, min_coverage, selected, groups, verified, method="greedy"):
    run_id = _next_run_id(m, n, k, j, s)
    count  = len(groups)
    fname  = f"{m}-{n}-{k}-{j}-{s}-{run_id}-{count}.txt"
    fpath  = os.path.join(RESULTS_DIR, fname)

    method_label = "Exact (Optimal)" if method == "exact" else "Greedy (Near-Optimal)"
    lines = [
        "=" * 52,
        "   Optimal Samples Selection System — Result",
        "=" * 52,
        f"Parameters  : m={m}, n={n}, k={k}, j={j}, s={s}",
        f"Min Coverage: {min_coverage}",
        f"Run ID      : {run_id}",
        f"Algorithm   : {method_label}",
        f"Coverage OK : {'YES' if verified else 'NO'}",
        "",
        f"Selected Samples ({n} from {m}):",
        f"  {selected}",
        "",
        f"Valid k-groups ({count} total):",
    ]
    for idx, grp in enumerate(groups, 1):
        lines.append(f"  {idx:3d}. {list(grp)}")
    lines += ["", "=" * 52]

    with open(fpath, "w", encoding="utf-8") as f:
        f.write("\n".join(lines))

    return fname, run_id, count


# ────────────────────────────────────────────────────────────────────────────
# Routes — S1 (Main Screen)
# ────────────────────────────────────────────────────────────────────────────

@app.route("/")
def index():
    return render_template("index.html")


@app.route("/run", methods=["POST"])
def run():
    """Execute algorithm, return JSON result (does NOT save to DB)."""
    data = request.get_json()
    try:
        m   = int(data["m"])
        n   = int(data["n"])
        k   = int(data["k"])
        j   = int(data["j"])
        s   = int(data["s"])
        cov = max(1, int(data.get("min_coverage", 1)))
        manual_raw = data.get("manual_input", "").strip()
        manual = (
            [int(x) for x in manual_raw.replace(" ", "").split(",") if x]
            if manual_raw else None
        )
    except (KeyError, ValueError) as e:
        return jsonify({"error": f"Invalid input: {e}"}), 400

    errors = _validate(m, n, k, j, s)
    if manual is not None:
        if len(manual) != n:
            errors.append(f"Manual input must have exactly {n} numbers")
        elif len(set(manual)) != len(manual):
            errors.append("Manual input contains duplicates")
        elif any(x < 1 or x > m for x in manual):
            errors.append(f"Manual input values must be between 1 and {m}")
    if errors:
        return jsonify({"error": "\n".join(errors)}), 400

    try:
        selected, groups, verified, method = _run_algorithm(m, n, k, j, s, cov, manual)
    except Exception as e:
        return jsonify({"error": str(e)}), 500

    groups_list = [list(g) for g in groups]
    return jsonify({
        "selected": selected,
        "groups":   groups_list,
        "count":    len(groups_list),
        "verified": verified,
        "method":   method,
        "params":   {"m": m, "n": n, "k": k, "j": j, "s": s, "min_coverage": cov}
    })


@app.route("/store", methods=["POST"])
def store():
    """Save the current result to the DB (results folder)."""
    data = request.get_json()
    try:
        m        = int(data["m"])
        n        = int(data["n"])
        k        = int(data["k"])
        j        = int(data["j"])
        s        = int(data["s"])
        cov      = max(1, int(data.get("min_coverage", 1)))
        selected = data["selected"]
        groups   = [tuple(g) for g in data["groups"]]
        verified = data.get("verified", True)
        method   = data.get("method", "greedy")
    except (KeyError, ValueError) as e:
        return jsonify({"error": f"Bad data: {e}"}), 400

    try:
        fname, run_id, count = _save_result(
            m, n, k, j, s, cov, selected, groups, verified, method
        )
    except Exception as e:
        return jsonify({"error": str(e)}), 500

    return jsonify({"filename": fname, "run_id": run_id, "count": count})


# ────────────────────────────────────────────────────────────────────────────
# Routes — S2 (Database Screen)
# ────────────────────────────────────────────────────────────────────────────

@app.route("/db")
def db():
    files = sorted(
        [f for f in os.listdir(RESULTS_DIR) if f.endswith(".txt")],
        reverse=True
    )
    entries = []
    for fname in files:
        stem  = fname.replace(".txt", "")
        parts = stem.split("-")
        if len(parts) == 7:
            try:
                m2,n2,k2,j2,s2,run,cnt = (int(p) for p in parts)
                label = (f"m={m2}  n={n2}  k={k2}  j={j2}  s={s2}"
                         f"   Run #{run}   ({cnt} groups)")
            except ValueError:
                label = fname
        else:
            label = fname
        entries.append({"filename": fname, "label": label})
    return render_template("db.html", entries=entries)


@app.route("/result/<filename>")
def result(filename):
    fpath = os.path.join(RESULTS_DIR, filename)
    if not os.path.isfile(fpath):
        return "File not found", 404
    content = open(fpath, encoding="utf-8").read()
    return render_template("result.html", filename=filename, content=content)


@app.route("/delete/<filename>", methods=["POST"])
def delete(filename):
    fpath = os.path.join(RESULTS_DIR, filename)
    if os.path.isfile(fpath):
        os.remove(fpath)
    return redirect(url_for("db"))


# ────────────────────────────────────────────────────────────────────────────

if __name__ == "__main__":
    # host="0.0.0.0" lets phones on the same WiFi access it
    print("=" * 52)
    print("  Optimal Samples Selection System")
    print("  http://localhost:5000")
    print("  Mobile: use your LAN IP:5000")
    print("=" * 52)
    app.run(host="0.0.0.0", port=8080, debug=False)

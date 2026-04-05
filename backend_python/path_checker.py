import os

def get_backend_and_result_paths():
    """
    Traverses upward to locate 'backend_python' and 'frontend_csharp/bin/Debug/results' directories.
    """
    base_dir = os.getcwd()

    while True:
        backend_path = os.path.join(base_dir, 'backend_python')
        result_path = os.path.join(base_dir, 'frontend_csharp', 'bin', 'Debug', 'results')

        if os.path.isdir(backend_path) and os.path.isdir(os.path.dirname(result_path)):
            os.makedirs(result_path, exist_ok=True)
            return backend_path, result_path

        new_base = os.path.dirname(base_dir)
        if new_base == base_dir:
            raise FileNotFoundError("❌ Could not locate required folders: 'backend_python' and 'frontend_csharp/bin/Debug'")
        base_dir = new_base

if __name__ == "__main__":
    try:
        backend, results = get_backend_and_result_paths()
        print(f"✅ Backend Path: {backend}")
        print(f"📂 Results Path: {results}")
    except Exception as e:
        print(f"❌ Error: {e}")

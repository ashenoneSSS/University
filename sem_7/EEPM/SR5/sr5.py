import numpy as np

flows = np.array([[650.0, 370.0],
                  [300.0, 350.0]])
consumption = np.array([750.0, 550.0])
value_added = np.array([0.5, 0.7])

A = np.array([[0.45, 0.30, 0.20],
              [0.20, 0.25, 0.40],
              [0.30, 0.35, 0.30]])
y = np.array([100.0, 70.0, 110.0])
tolerance = 0.01


def task_1():
    print("Завдання 8.1")
    gross = flows.sum(axis=1) + consumption
    a2 = flows / gross
    print(f"  повні випуски: X1 = {gross[0]:.0f}, X2 = {gross[1]:.0f}")
    print("  матриця прямих витрат A = x_ij / X_j:")
    for row in a2:
        print("   ", "  ".join(f"{v:.4f}" for v in row))
    prices = np.linalg.solve(np.eye(2) - a2.T, value_added)
    print(f"  ціни p = (E - Aᵀ)⁻¹ · s: p1 = {prices[0]:.4f}, p2 = {prices[1]:.4f}")
    check = a2.T @ prices + value_added
    print(f"  перевірка p = Aᵀp + s: {check[0]:.4f}, {check[1]:.4f}")


def task_2():
    print("\nЗавдання 8.2")
    poly = np.poly(A)
    print("  характеристичний поліном det(λE - A) = λ³ "
          f"{poly[1]:+.6f}·λ² {poly[2]:+.6f}·λ {poly[3]:+.6f}")
    eigenvalues, right = np.linalg.eig(A)
    for lam in eigenvalues:
        print(f"  λ = {lam:.6f}" if abs(lam.imag) > 1e-12 else f"  λ = {lam.real:.6f}")
    k = int(np.argmax(eigenvalues.real))
    frob = eigenvalues[k].real
    x_right = np.abs(right[:, k].real)
    x_right /= x_right.sum()
    w, left = np.linalg.eig(A.T)
    x_left = np.abs(left[:, int(np.argmax(w.real))].real)
    x_left /= x_left.sum()
    print(f"  число Фробеніуса λ* = {frob:.6f}")
    print("  правий вектор Фробеніуса (нормований):", np.round(x_right, 4))
    print("  лівий вектор Фробеніуса (нормований):", np.round(x_left, 4))
    print(f"  λ* {'< 1 — матриця продуктивна' if frob < 1 else '≥ 1 — матриця непродуктивна'}")
    print(f"  суми стовпців: {np.round(A.sum(axis=0), 3)}, максимальна {A.sum(axis=0).max():.2f} < 1")

    B = np.linalg.inv(np.eye(3) - A)
    print("\n  матриця повних витрат B = (E - A)⁻¹:")
    for row in B:
        print("   ", "  ".join(f"{v:8.4f}" for v in row))

    total = np.eye(3)
    power = np.eye(3)
    n = 0
    while np.abs(B - total).max() >= tolerance:
        n += 1
        power = power @ A
        total = total + power
    print(f"\n  ряд E + A + ... + A^N: максимальне відхилення від B < {tolerance} при N = {n}"
          f" (відхилення {np.abs(B - total).max():.5f})")
    for m in (1, 5, 10, 20, n):
        s = sum(np.linalg.matrix_power(A, i) for i in range(m + 1))
        print(f"    N = {m:3d}: max|B - S_N| = {np.abs(B - s).max():.5f}")

    x = B @ y
    print(f"\n  вектор кінцевого випуску x = B·y = ({x[0]:.2f}; {x[1]:.2f}; {x[2]:.2f})")
    print(f"  перевірка (E - A)x = {np.round((np.eye(3) - A) @ x, 4)}")


if __name__ == "__main__":
    task_1()
    task_2()

import numpy as np
import matplotlib.pyplot as plt
from scipy.optimize import minimize

K = np.array([3700, 3620, 3695, 3860, 3930, 3650, 3700, 3750, 3870, 3810], dtype=float)
L = np.array([10235, 10095, 10225, 10325, 10615, 10055, 10125, 10135, 10435, 10475], dtype=float)
q = np.array([20236, 19920, 20390, 20620, 21100, 19920, 20080, 20180, 20790, 20745], dtype=float)
p, wK, wL = 9.0, 2.0, 2.0
TC, q0 = 27000.0, 38500.0


def fit_cobb_douglas():
    X = np.column_stack([np.ones_like(K), np.log(K), np.log(L)])
    y = np.log(q)
    coef, *_ = np.linalg.lstsq(X, y, rcond=None)
    fitted = X @ coef
    r2_log = 1 - np.sum((y - fitted) ** 2) / np.sum((y - y.mean()) ** 2)
    A, alpha, beta = np.exp(coef[0]), coef[1], coef[2]
    q_hat = A * K ** alpha * L ** beta
    r2 = 1 - np.sum((q - q_hat) ** 2) / np.sum((q - q.mean()) ** 2)
    mape = np.mean(np.abs(q - q_hat) / q) * 100
    return A, alpha, beta, r2_log, r2, mape


def main():
    A, a, b, r2_log, r2, mape = fit_cobb_douglas()
    F = lambda k, l: A * k ** a * l ** b
    print(f"F(K, L) = {A:.4f} · K^{a:.4f} · L^{b:.4f}")
    print(f"R² (ln q) = {r2_log:.4f}, R² (q) = {r2:.4f}, середня відносна похибка = {mape:.2f}%")
    print(f"α + β = {a + b:.4f} → {'спадна' if a + b < 1 else 'зростаюча' if a + b > 1 else 'стала'} віддача від масштабу")

    print("\n1) Максимізація прибутку (короткостроковий період)")
    k_max, l_max = K.max(), L.max()
    print(f"   обмеження: K ≤ {k_max:.0f}, L ≤ {l_max:.0f}")
    profit = lambda v: -(p * F(v[0], v[1]) - wK * v[0] - wL * v[1])
    res = minimize(profit, x0=[K.mean(), L.mean()], bounds=[(1, k_max), (1, l_max)])
    k1, l1 = res.x
    q1 = F(k1, l1)
    print(f"   p·MPK = {p * a * q1 / k1:.3f} > wK = {wK},  p·MPL = {p * b * q1 / l1:.3f} > wL = {wL}")
    print(f"   K* = {k1:.0f}, L* = {l1:.0f}, q* = {q1:.0f}, прибуток = {-res.fun:.0f}")
    scale = (p * A * (a / wK) ** a * (b / wL) ** b) ** (1 / (1 - a - b))
    print(f"   без обмежень оптимум: K = {a / wK * scale:.3e}, L = {b / wL * scale:.3e} — далеко за межами ресурсів")

    print("\n2) Максимізація випуску при TC = wK·K + wL·L = %.0f" % TC)
    k2 = a / (a + b) * TC / wK
    l2 = b / (a + b) * TC / wL
    print(f"   K* = α/(α+β)·TC/wK = {k2:.1f}, L* = β/(α+β)·TC/wL = {l2:.1f}")
    print(f"   q_max = {F(k2, l2):.1f}, MRTS = (α·L)/(β·K) = {a * l2 / (b * k2):.3f} = wK/wL = {wK / wL:.3f}")

    print("\n3) Мінімізація витрат при випуску q0 = %.0f" % q0)
    ratio = a * wL / (b * wK)
    l3 = (q0 / (A * ratio ** a)) ** (1 / (a + b))
    k3 = ratio * l3
    print(f"   K/L = α·wL/(β·wK) = {ratio:.4f}")
    print(f"   K* = {k3:.1f}, L* = {l3:.1f}, F(K*, L*) = {F(k3, l3):.1f}, TC_min = {wK * k3 + wL * l3:.1f}")

    kk, ll = np.meshgrid(np.linspace(2500, 9000, 300), np.linspace(8000, 25000, 300))
    fig, ax = plt.subplots(figsize=(9, 6))
    for level, color in ((F(k1, l1), "tab:green"), (F(k2, l2), "tab:blue"), (q0, "tab:purple")):
        cs = ax.contour(kk, ll, F(kk, ll), levels=[level], colors=[color])
        ax.clabel(cs, fmt="q=%d")
    kline = np.linspace(2500, 9000, 50)
    ax.plot(kline, (TC - wK * kline) / wL, "tab:blue", ls="--", label=f"ізокоста TC = {TC:.0f}")
    ax.plot(kline, (wK * k3 + wL * l3 - wK * kline) / wL, "tab:purple", ls="--", label="мінімальна ізокоста для q0")
    ax.scatter(K, L, c="gray", s=15, label="спостереження")
    ax.plot(k1, l1, "o", c="tab:green", label="1) max прибутку")
    ax.plot(k2, l2, "o", c="tab:blue", label="2) max випуску")
    ax.plot(k3, l3, "o", c="tab:purple", label="3) min витрат")
    ax.set_xlabel("K (капітал)")
    ax.set_ylabel("L (праця)")
    ax.set_title("Ізокванти та ізокости")
    ax.set_xlim(2500, 9000)
    ax.set_ylim(8000, 25000)
    ax.grid(alpha=0.3)
    ax.legend(loc="upper right")
    fig.tight_layout()
    plt.show()


if __name__ == "__main__":
    main()

import numpy as np
import matplotlib.pyplot as plt

price = np.array([1.6, 2.3, 3.7, 4.4, 5.8, 6.3, 7.1, 8.0])
demand = np.array([600, 520, 440, 380, 330, 290, 220, 180])
supply = np.array([240, 300, 365, 402, 520, 580, 620, 660])
quota = 300


def r_squared(y, fitted):
    return 1 - np.sum((y - fitted) ** 2) / np.sum((y - y.mean()) ** 2)


def fit_models(y):
    models = {}
    a, b = np.polyfit(price, y, 1)
    models["лінійна Q = a + bP"] = r_squared(y, a * price + b)
    k, c = np.polyfit(price, np.log(y), 1)
    models["експоненціальна Q = a·e^(bP)"] = r_squared(y, np.exp(c) * np.exp(k * price))
    k, c = np.polyfit(np.log(price), np.log(y), 1)
    models["степенева Q = a·P^b"] = r_squared(y, np.exp(c) * price ** k)
    k, c = np.polyfit(np.log(price), y, 1)
    models["логарифмічна Q = a + b·lnP"] = r_squared(y, k * np.log(price) + c)
    return models


def main():
    for name, y in (("Попит", demand), ("Пропозиція", supply)):
        print(f"{name}:")
        for model, r2 in fit_models(y).items():
            print(f"  {model:32s} R² = {r2:.4f}")

    bd, ad = np.polyfit(price, demand, 1)
    bs, as_ = np.polyfit(price, supply, 1)
    print(f"\nQd(P) = {ad:.2f} {bd:+.3f}·P")
    print(f"Qs(P) = {as_:.2f} {bs:+.3f}·P")

    pe = (ad - as_) / (bs - bd)
    qe = ad + bd * pe
    print(f"\nРівновага: P* = {pe:.3f}, Q* = {qe:.2f}")

    print("\nСтабільність:")
    print(f"  Вальрас: d(Qd - Qs)/dP = {bd - bs:.3f} {'< 0 — стійка' if bd - bs < 0 else '> 0 — нестійка'}")
    marshall = 1 / bd - 1 / bs
    print(f"  Маршалл: d(Pd - Ps)/dQ = {marshall:.4f} {'< 0 — стійка' if marshall < 0 else '> 0 — нестійка'}")
    ratio = abs(bs / bd)
    print(f"  Павутиноподібна модель: |bs / bd| = {ratio:.3f} "
          f"{'< 1 — коливання затухають' if ratio < 1 else '> 1 — коливання розходяться'}")

    demand_price = lambda q: (q - ad) / bd
    supply_price = lambda q: (q - as_) / bs
    pmax = -ad / bd

    pd_quota = demand_price(quota)
    ps_quota = supply_price(quota)
    print(f"\nКвота Qlim = {quota}:")
    print(f"  ціна покупця Pd = {pd_quota:.3f} ({(pd_quota / pe - 1) * 100:+.1f}%)")
    print(f"  мінімальна ціна продавця Ps = {ps_quota:.3f}")
    print(f"  обсяг {qe:.2f} → {quota} ({(quota / qe - 1) * 100:+.1f}%)")
    print(f"  дефіцит за старої ціни P* : {qe - quota:.2f}")
    print(f"  квотна рента = {(pd_quota - ps_quota) * quota:.2f}")

    cs0 = 0.5 * (pmax - pe) * qe
    ps0 = pe * as_ + 0.5 * pe * (qe - as_)
    cs1 = 0.5 * (pmax - pd_quota) * quota
    ps1 = pd_quota * as_ + (pd_quota - 0.5 * ps_quota) * (quota - as_)
    dwl = 0.5 * (pd_quota - ps_quota) * (qe - quota)
    print(f"\n  надлишок споживача: {cs0:.1f} → {cs1:.1f}")
    print(f"  надлишок виробника (з рентою): {ps0:.1f} → {ps1:.1f}")
    print(f"  чисті втрати суспільства: {dwl:.1f}")

    q = np.linspace(0, 750, 400)
    fig, ax = plt.subplots(figsize=(9, 6))
    ax.scatter(demand, price, c="tab:blue", label="Попит (дані)")
    ax.scatter(supply, price, c="tab:orange", label="Пропозиція (дані)")
    ax.plot(q, demand_price(q), c="tab:blue", label=f"D: P = ({ad:.1f} − Q)/{-bd:.2f}")
    ax.plot(q, supply_price(q), c="tab:orange", label=f"S: P = (Q − {as_:.1f})/{bs:.2f}")
    ax.vlines(quota, supply_price(quota), 11, colors="tab:red", linestyles="--", label=f"Квота Qlim = {quota}")
    ax.plot(qe, pe, "ko")
    ax.annotate(f"E ({qe:.1f}; {pe:.2f})", (qe, pe), textcoords="offset points", xytext=(10, -15))
    ax.plot(quota, pd_quota, "ro")
    ax.annotate(f"E₁ ({quota}; {pd_quota:.2f})", (quota, pd_quota), textcoords="offset points", xytext=(10, 5))
    ax.fill_between([quota, qe], [ps_quota, pe], [pd_quota, pe], color="tab:red", alpha=0.15, label="Чисті втрати")
    ax.set_xlim(0, 750)
    ax.set_ylim(0, 11)
    ax.set_xlabel("Q")
    ax.set_ylabel("P")
    ax.set_title("Ринкова рівновага та вплив квоти")
    ax.grid(alpha=0.3)
    ax.legend()
    fig.tight_layout()
    plt.show()


if __name__ == "__main__":
    main()

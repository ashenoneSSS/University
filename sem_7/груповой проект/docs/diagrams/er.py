"""Генерує ER-діаграму бази AutoMarket у SVG (docs/diagrams/er.svg).

Запуск: python3 docs/diagrams/er.py
Сутності відповідають AutoMarket.Api/Models/Entities.cs.
"""
from pathlib import Path
from xml.sax.saxutils import escape

# kind: pk / fk / pkfk / col / nav (навігаційна властивість EF, у БД колонки немає)
ENTITIES = {
    "CATEGORY": (40, 90, [
        ("pk", "Id", "int"),
        ("col", "Name", "string"),
        ("nav", "Listings", "List<Listing>"),
    ]),
    "LISTING": (560, 90, [
        ("pk", "Id", "int"),
        ("col", "Title", "string"),
        ("col", "Description", "string"),
        ("col", "Price", "decimal"),
        ("col", "City", "string?"),
        ("col", "Status", "ListingStatus"),
        ("col", "RejectReason", "string?"),
        ("col", "Views", "int"),
        ("col", "SearchText", "string"),
        ("col", "CityKey", "string?"),
        ("col", "CreatedAt", "DateTime"),
        ("col", "UpdatedAt", "DateTime?"),
        ("fk", "UserId", "int"),
        ("nav", "User", "User"),
        ("fk", "CategoryId", "int"),
        ("nav", "Category", "Category"),
        ("nav", "Photos", "List<Photo>"),
    ]),
    "USER": (1080, 90, [
        ("pk", "Id", "int"),
        ("col", "Email", "string"),
        ("col", "PasswordHash", "string"),
        ("col", "Name", "string"),
        ("col", "Phone", "string?"),
        ("col", "Role", "UserRole"),
        ("col", "IsBlocked", "bool"),
        ("col", "CreatedAt", "DateTime"),
        ("nav", "Listings", "List<Listing>"),
        ("nav", "Favorites", "List<Favorite>"),
    ]),
    "PHOTO": (40, 470, [
        ("pk", "Id", "int"),
        ("col", "Url", "string"),
        ("fk", "ListingId", "int"),
        ("nav", "Listing", "Listing"),
    ]),
    "FAVORITE": (1080, 590, [
        ("pkfk", "UserId", "int"),
        ("pkfk", "ListingId", "int"),
        ("col", "CreatedAt", "DateTime"),
        ("nav", "User", "User"),
        ("nav", "Listing", "Listing"),
    ]),
}

W, HEADER, ROW, PAD = 420, 46, 32, 14
CANVAS_W, CANVAS_H = 1540, 900
COLORS = {"pk": "#b45309", "fk": "#047857", "pkfk": "#b45309", "col": "#111827", "nav": "#6d28d9"}
RED, BLUE, BORDER, BODY = "#d03a2f", "#3b63e0", "#26398a", "#eef4fd"
MONO = "Menlo, Consolas, 'DejaVu Sans Mono', monospace"
SANS = "'Helvetica Neue', Arial, sans-serif"


def height(rows):
    return HEADER + len(rows) * ROW + PAD


def box(name, x, y, rows):
    h = height(rows)
    out = [
        f'<rect x="{x}" y="{y}" width="{W}" height="{h}" rx="7" fill="{BODY}" stroke="{BORDER}" stroke-width="4"/>',
        f'<path d="M{x} {y + 7} a7 7 0 0 1 7 -7 h{W - 14} a7 7 0 0 1 7 7 v{HEADER - 7} h{-W} z" fill="{BLUE}" stroke="{BORDER}" stroke-width="4"/>',
        f'<text x="{x + W / 2}" y="{y + 31}" text-anchor="middle" font-family="{SANS}" font-size="21" font-weight="700" fill="#fff">{name}</text>',
    ]
    for i, (kind, field, typ) in enumerate(rows):
        ty = y + HEADER + 24 + i * ROW
        color = COLORS[kind]
        key = {"pk": "PK", "fk": "FK", "pkfk": "PK"}.get(kind, "")
        if kind == "pkfk":  # складений ключ: PK і FK одночасно
            out.append(f'<text x="{x + 18}" y="{ty}" font-family="{MONO}" font-size="15" fill="{COLORS["pk"]}">PK<tspan fill="{COLORS["fk"]}">,FK</tspan></text>')
            color = COLORS["fk"]
        elif key:
            out.append(f'<text x="{x + 18}" y="{ty}" font-family="{MONO}" font-size="15" fill="{color}">{key}</text>')
        out.append(f'<text x="{x + 80}" y="{ty}" font-family="{MONO}" font-size="15" fill="{color}">{escape(field)}</text>')
        out.append(f'<text x="{x + 250}" y="{ty}" font-family="{MONO}" font-size="15" fill="{color}">{escape(typ)}</text>')
    return out


def arrow(x1, y1, x2, y2, label, lx, ly):
    return [
        f'<line x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}" stroke="{RED}" stroke-width="4" marker-end="url(#head)"/>',
        f'<rect x="{lx - 26}" y="{ly - 15}" width="52" height="28" rx="4" fill="#fff" stroke="{RED}" stroke-width="2"/>',
        f'<text x="{lx}" y="{ly + 6}" text-anchor="middle" font-family="{SANS}" font-size="16" font-weight="700" fill="{RED}">{label}</text>',
    ]


def legend(x, y):
    items = [
        (f'<rect x="{x + 14}" y="{y + 12}" width="22" height="20" fill="{BLUE}"/>', "Сутність / таблиця"),
        (f'<text x="{x + 12}" y="{y + 61}" font-family="{SANS}" font-size="22" font-weight="700" font-style="italic" fill="{COLORS["pk"]}">PK</text>', "Первинний ключ"),
        (f'<text x="{x + 12}" y="{y + 93}" font-family="{SANS}" font-size="22" font-weight="700" font-style="italic" fill="{COLORS["fk"]}">FK</text>', "Зовнішній ключ (колонка в БД)"),
        (f'<text x="{x + 16}" y="{y + 126}" font-family="{SANS}" font-size="26" font-weight="700" font-style="italic" fill="{COLORS["nav"]}">N</text>', "Навігаційна властивість (лише EF)"),
        (f'<line x1="{x + 10}" y1="{y + 150}" x2="{x + 44}" y2="{y + 150}" stroke="{RED}" stroke-width="4"/>', "Зв'язок 1:N"),
    ]
    out = [f'<rect x="{x}" y="{y}" width="380" height="172" rx="4" fill="#fff" stroke="#c7c7c7" stroke-width="1.5"/>']
    for i, (icon, text) in enumerate(items):
        out.append(icon)
        out.append(f'<text x="{x + 60}" y="{y + 28 + i * 32}" font-family="{SANS}" font-size="17" fill="#111827">{text}</text>')
    return out


parts = [
    f'<svg xmlns="http://www.w3.org/2000/svg" width="{CANVAS_W}" height="{CANVAS_H}" viewBox="0 0 {CANVAS_W} {CANVAS_H}">',
    f'<defs><marker id="head" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="5" markerHeight="5" orient="auto-start-reverse"><path d="M0 0 L10 5 L0 10 z" fill="{RED}"/></marker></defs>',
    f'<rect width="100%" height="100%" fill="#fff"/>',
    f'<text x="{CANVAS_W / 2}" y="50" text-anchor="middle" font-family="{SANS}" font-size="32" font-weight="700" fill="#000">AutoMarket — ER Diagram</text>',
]

# зв'язки (стрілка вказує на сторону «багато»)
cat, lst, usr, pho, fav = (ENTITIES[k] for k in ("CATEGORY", "LISTING", "USER", "PHOTO", "FAVORITE"))
parts += arrow(cat[0] + W, 170, lst[0] - 4, 170, "1:N", (cat[0] + W + lst[0]) / 2, 140)          # Category → Listing
parts += arrow(usr[0], 170, lst[0] + W + 4, 170, "1:N", (lst[0] + W + usr[0]) / 2, 140)          # User → Listing
parts += arrow(lst[0], 560, pho[0] + W + 4, 560, "1:N", (pho[0] + W + lst[0]) / 2, 530)          # Listing → Photo
parts += arrow(lst[0] + W, 660, fav[0] - 4, 660, "1:N", (lst[0] + W + fav[0]) / 2, 630)          # Listing → Favorite
ux = usr[0] + W / 2
parts += arrow(ux, usr[1] + height(usr[2]), ux, fav[1] - 4, "1:N", ux + 50, (usr[1] + height(usr[2]) + fav[1]) / 2)  # User → Favorite

for name, (x, y, rows) in ENTITIES.items():
    parts += box(name, x, y, rows)

parts += legend(40, 700)
parts.append("</svg>")

out = Path(__file__).with_name("er.svg")
out.write_text("\n".join(parts), encoding="utf-8")
print(f"saved {out}")

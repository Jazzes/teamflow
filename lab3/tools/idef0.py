"""Генератор диаграмм IDEF0 в формате diagrams.net (.drawio).
Модель описывается данными (блоки и стрелки ICOM), геометрия и трассировка
стрелок вычисляются автоматически, результат открывается и правится в diagrams.net."""
from xml.sax.saxutils import escape

FONT = 15

class Diagram:
    def __init__(self, node, title, number, width, height):
        self.node, self.title, self.number = node, title, number
        self.W, self.H = width, height
        self.cells = []
        self.n = 2

    def _id(self):
        self.n += 1
        return f'c{self.n}'

    def rect(self, x, y, w, h, value='', style=''):
        self.cells.append(f'<mxCell id="{self._id()}" value="{escape(value, {chr(34): "&quot;"})}" style="{style}" vertex="1" parent="1">'
                          f'<mxGeometry x="{x:.0f}" y="{y:.0f}" width="{w:.0f}" height="{h:.0f}" as="geometry"/></mxCell>')

    def text(self, x, y, w, h, value, align='left', size=FONT, valign='top', bold=False, italic=False):
        st = (f'text;html=1;whiteSpace=wrap;align={align};verticalAlign={valign};fontSize={size};'
              f'fontFamily=Arial;spacing=0;{"fontStyle=1;" if bold else ""}{"fontStyle=2;" if italic else ""}')
        self.rect(x, y, w, h, value, st)

    def poly(self, pts, arrow=True, dashed=False):
        (x0, y0), (x1, y1) = pts[0], pts[-1]
        mids = ''.join(f'<mxPoint x="{x:.0f}" y="{y:.0f}"/>' for x, y in pts[1:-1])
        st = ('edgeStyle=none;html=1;rounded=0;strokeWidth=1.3;strokeColor=#000000;'
              + ('endArrow=block;endFill=1;endSize=7;' if arrow else 'endArrow=none;')
              + ('dashed=1;' if dashed else ''))
        self.cells.append(f'<mxCell id="{self._id()}" style="{st}" edge="1" parent="1"><mxGeometry relative="1" as="geometry">'
                          f'<mxPoint x="{x0:.0f}" y="{y0:.0f}" as="sourcePoint"/><mxPoint x="{x1:.0f}" y="{y1:.0f}" as="targetPoint"/>'
                          f'<Array as="points">{mids}</Array></mxGeometry></mxCell>')

    def xml(self, page_name):
        body = ''.join(self.cells)
        return (f'<diagram name="{escape(page_name)}" id="{escape(self.node)}"><mxGraphModel dx="1400" dy="900" grid="0" gridSize="10" '
                f'guides="1" tooltips="1" connect="0" arrows="1" fold="1" page="1" pageScale="1" pageWidth="{self.W}" pageHeight="{self.H}" '
                f'math="0" shadow="0"><root><mxCell id="0"/><mxCell id="1" parent="0"/>{body}</root></mxGraphModel></diagram>')


def frame(d, meta):
    """Рамка IDEF0: шапка с атрибутами и нижняя строка «Узел / Название / Номер»."""
    W, H = d.W, d.H
    st = 'rounded=0;whiteSpace=wrap;html=1;fillColor=none;strokeColor=#000000;strokeWidth=1;fontFamily=Arial;fontSize=13;align=left;verticalAlign=middle;spacingLeft=4;'
    top = 10
    cols = [('АВТОР: ' + meta['author'], 0.30), ('ПРОЕКТ: ' + meta['project'], 0.26), ('ДАТА: ' + meta['date'], 0.16), ('РАБОЧАЯ ВЕРСИЯ', 0.14), ('ЧИТАТЕЛЬ:', 0.14)]
    x = 10
    for label, k in cols:
        w = (W - 20) * k
        d.rect(x, top, w, 30, label, st)
        x += w
    d.rect(10, 40, W - 20, H - 40 - 50, '', 'rounded=0;html=1;fillColor=none;strokeColor=#000000;strokeWidth=1;')
    y = H - 50
    parts = [('УЗЕЛ:', d.node, 0.14), ('НАЗВАНИЕ:', d.title, 0.72), ('НОМЕР:', str(d.number), 0.14)]
    x = 10
    for cap, val, k in parts:
        w = (W - 20) * k
        d.rect(x, y, w, 40, f'<font style="font-size:11px">{cap}</font><br><b>{escape(val)}</b>',
               'rounded=0;whiteSpace=wrap;html=1;fillColor=none;strokeColor=#000000;fontFamily=Arial;fontSize=16;align=center;verticalAlign=middle;')
        x += w
    return 40, H - 50  # верх и низ рабочей области


def box(d, x, y, w, h, name, number, big=False):
    d.rect(x, y, w, h, name,
           f'rounded=0;whiteSpace=wrap;html=1;fillColor=#FFFFFF;strokeColor=#000000;strokeWidth=1.6;fontFamily=Arial;'
           f'fontSize={19 if big else 16};fontStyle=1;spacing=6;')
    d.text(x + w - 50, y + h - 20, 46, 17, number, align='right', size=13, bold=True)


def ports(start, length, n):
    return [start + length * (i + 1) / (n + 1) for i in range(n)]


def build(spec, meta):
    """spec: node, title, number, boxes[{id,name,num}], arrows[...]
    Стрелка: {'label', 'code' (для граничных), 'from': 'I'|'C'|'M'| box_id, 'to': [ (box_id, side) | 'O' ], 'lbl': (dx,dy) опционально}"""
    boxes = spec['boxes']
    n = len(boxes)
    bw, bh = spec.get('bw', 190), spec.get('bh', 92)
    dx, dy = bw + spec.get('gapx', 95), bh + spec.get('gapy', 48)
    left0, top0 = spec.get('left0', 250), spec.get('top0', 190)
    W = int(left0 + (n - 1) * dx + bw + spec.get('right', 270))
    H = int(top0 + (n - 1) * dy + bh + spec.get('bottom', 190))
    d = Diagram(spec['node'], spec['title'], spec['number'], W, H)
    ytop, ybot = frame(d, meta)
    xl, xr = 10, W - 10
    geo = {}
    for k, b in enumerate(boxes):
        geo[b['id']] = dict(x=left0 + k * dx, y=top0 + k * dy, w=bw, h=bh, k=k)
        box(d, left0 + k * dx, top0 + k * dy, bw, bh, b['name'], b['num'])

    # раздаём порты по сторонам блоков
    side_items = {}
    for ai, a in enumerate(spec['arrows']):
        src = a['from']
        if src in geo and not a.get('same_port'):
            side_items.setdefault((src, 'O'), []).append(('src', ai))
        for t in a['to']:
            if t != 'O':
                side_items.setdefault((t[0], t[1]), []).append(('dst', ai))
    port = {}
    for (bid, side), items in side_items.items():
        g = geo[bid]
        uniq = []
        for it in items:
            if it[1] not in [u[1] for u in uniq] or it[0] == 'dst':
                uniq.append(it)
        # порядок задаётся в спецификации через 'order', иначе порядком стрелок
        uniq.sort(key=lambda it: spec['arrows'][it[1]].get('order', {}).get(f'{bid}{side}', it[1]))
        if side in ('I', 'O'):
            ps = ports(g['y'], g['h'], len(uniq))
        else:
            ps = ports(g['x'], g['w'], len(uniq))
        for it, p in zip(uniq, ps):
            port[(bid, side, it[1])] = p

    for ai, a in enumerate(spec['arrows']):
        if a.get('same_port'):
            port[(a['from'], 'O', ai)] = port[(a['from'], 'O', ai - 1)]
    # точка слияния (join) нескольких выходов в одну граничную стрелку
    joins = [ai for ai, a in enumerate(spec['arrows']) if a.get('out_y') == 'J']
    if joins:
        last = max(joins, key=lambda ai: geo[spec['arrows'][ai]['from']]['k'])
        y_join = port[(spec['arrows'][last]['from'], 'O', last)]
        g_last = geo[spec['arrows'][last]['from']]
        x_join = g_last['x'] + g_last['w'] + spec.get('join_dx', 40)
        for ai in joins:
            spec['arrows'][ai]['out_y'] = y_join
            spec['arrows'][ai]['join_x'] = x_join
    lane_used = {}
    def lane(key, base, step, direction):
        k = lane_used.get(key, 0)
        lane_used[key] = k + 1
        return base + direction * step * k

    labels = []
    for ai, a in enumerate(spec['arrows']):
        src = a['from']
        segs_for_label = []
        code = a.get('code', '')
        text = (f'<b>{code}</b> ' if code else '') + escape(a['label'])
        for t in a['to']:
            if src == 'I':
                bid = t[0]; g = geo[bid]; y = port[(bid, 'I', ai)]
                pts = [(xl, y), (g['x'], y)]
            elif src == 'C':
                bid = t[0]; g = geo[bid]; x = port[(bid, 'C', ai)]
                pts = [(x, ytop), (x, g['y'])]
            elif src == 'M':
                bid = t[0]; g = geo[bid]; x = port[(bid, 'M', ai)]
                pts = [(x, ybot), (x, g['y'] + g['h'])]
            else:
                gs = geo[src]; ys = port[(src, 'O', ai)]; xs = gs['x'] + gs['w']
                if t == 'O':
                    yo = a.get('out_y', ys)
                    if yo == ys:
                        pts = [(xs, ys), (xr, ys)]
                    else:
                        xj = a.get('join_x') or (xr - 60)
                        pts = [(xs, ys), (xj, ys), (xj, yo), (xr, yo)]
                else:
                    bid, side = t; g = geo[bid]
                    if side == 'I':
                        yt = port[(bid, 'I', ai)]
                        if g['k'] > gs['k']:
                            xm = lane(('I', bid), g['x'] - 22, 16, -1)
                            pts = [(xs, ys), (xm, ys), (xm, yt), (g['x'], yt)]
                        else:  # обратная связь по входу: обход снизу
                            xo = lane(('fbO', src), xs + 22, 16, 1)
                            yb = lane(('fbY',), max(gg['y'] + gg['h'] for gg in geo.values()) + 30, 38, 1)
                            xm = lane(('fbI', bid), g['x'] - 28, 16, -1)
                            pts = [(xs, ys), (xo, ys), (xo, yb), (xm, yb), (xm, yt), (g['x'], yt)]
                    elif side == 'C':
                        xt = port[(bid, 'C', ai)]
                        if g['k'] > gs['k']:
                            pts = [(xs, ys), (xt, ys), (xt, g['y'])]
                        else:  # обратная связь по управлению: обход сверху
                            xo = lane(('fbO', src), xs + 22, 16, 1)
                            yt_ = lane(('fbC', bid), g['y'] - 26, 14, -1)
                            pts = [(xs, ys), (xo, ys), (xo, yt_), (xt, yt_), (xt, g['y'])]
                    elif side == 'M':
                        xt = port[(bid, 'M', ai)]
                        yb = g['y'] + g['h'] + 30
                        pts = [(xs, ys), (xs + 20, ys), (xs + 20, yb), (xt, yb), (xt, g['y'] + g['h'])]
            d.poly(pts)
            segs_for_label.append(pts)
        labels.append((a, text, segs_for_label, src))

    # подписи стрелок
    for a, text, segs, src in labels:
        if a.get('nolabel'):
            continue
        if a.get('join_x') is not None:
            text = escape(a['label'])
        lw = a.get('lw', 170)
        ox, oy = a.get('lbl', (0, 0))
        code = a.get('code', '')
        for bi, pts in enumerate(segs):
            if bi > 0:
                # ответвления граничных стрелок подписываем только кодом ICOM
                if src == 'C':
                    d.text(pts[0][0] + 5, ytop + 6, 40, 20, f'<b>{code}</b>')
                elif src == 'M':
                    d.text(pts[0][0] + 5, ybot - 24, 40, 20, f'<b>{code}</b>')
                elif src == 'I':
                    d.text(xl + 8, pts[0][1] - 22, 40, 20, f'<b>{code}</b>')
                continue
            if src == 'I':
                d.text(xl + 8 + ox, pts[0][1] - 42 + oy, lw, 40, text, valign='bottom')
            elif src == 'C':
                if a.get('cside') == 'left':
                    d.text(pts[0][0] - 5 - lw + ox, ytop + 6 + oy, lw, 60, text, align='right')
                else:
                    d.text(pts[0][0] + 5 + ox, ytop + 6 + oy, lw, 60, text)
            elif src == 'M':
                x = pts[0][0]
                if a.get('mside') == 'left':
                    d.text(x - 5 - lw + ox, ybot - 64 + oy, lw, 60, text, align='right', valign='bottom')
                else:
                    d.text(x + 5 + ox, ybot - 64 + oy, lw, 60, text, valign='bottom')
            else:
                if a.get('join_x') is not None and a['to'][0] == 'O':
                    p0 = pts[0]
                    d.text(p0[0] + 6 + ox, p0[1] - 42 + oy, lw, 40, escape(a['label']), valign='bottom')
                    if a.get('boundary'):
                        p1 = pts[-1]
                        d.text(p1[0] - 230, p1[1] + 4, 222, 24, a['boundary'], align='right', valign='top')
                elif a['to'][0] == 'O':
                    p1 = pts[-1]
                    d.text(p1[0] - lw - 8 + ox, p1[1] - 42 + oy, lw, 40, text, align='right', valign='bottom')
                elif 'seg' in a:
                    (x0, y0), (x1, y1) = pts[a['seg']], pts[a['seg'] + 1]
                    if y0 == y1:
                        d.text(min(x0, x1) + 8 + ox, y0 - 42 + oy, lw, 40, text, valign='bottom')
                    else:
                        d.text(x0 + 6 + ox, (y0 + y1) / 2 - 20 + oy, lw, 40, text, valign='middle')
                else:
                    p0 = pts[0]
                    d.text(p0[0] + 6 + ox, p0[1] - 42 + oy, lw, 40, text, valign='bottom')
    return d


def build_context(spec, meta):
    """Контекстная диаграмма A-0: один блок и граничные стрелки."""
    W, H = spec.get('W', 1100), spec.get('H', 720)
    d = Diagram(spec['node'], spec['title'], spec['number'], W, H)
    ytop, ybot = frame(d, meta)
    bw, bh = spec.get('bw', 470), 190
    bx, by = (W - bw) / 2, ytop + spec.get('box_top', 170)
    box(d, bx, by, bw, bh, spec['box'], 'A0', big=True)
    groups = {'I': [], 'C': [], 'O': [], 'M': []}
    for a in spec['arrows']:
        groups[a['kind']].append(a)
    for kind, arr in groups.items():
        if kind in ('I', 'O'):
            ps = ports(by, bh, len(arr))
        else:
            ps = ports(bx, bw, len(arr))
        for a, p in zip(arr, ps):
            t = f'<b>{a["code"]}</b> {escape(a["label"])}'
            lw = a.get('lw', 260)
            if kind == 'I':
                d.poly([(10, p), (bx, p)]); d.text(24, p - 42, lw, 40, t, valign='bottom')
            elif kind == 'O':
                d.poly([(bx + bw, p), (W - 10, p)]); d.text(W - 24 - lw, p - 42, lw, 40, t, align='right', valign='bottom')
            elif kind == 'C':
                d.poly([(p, ytop), (p, by)]); d.text(p + 6 + a.get('dx', 0), ytop + 14 + a.get('dy', 0), lw, 60, t)
            else:
                d.poly([(p, ybot), (p, by + bh)])
                if a.get('mside') == 'left':
                    d.text(p - 6 - lw, ybot - 66, lw, 60, t, align='right', valign='bottom')
                else:
                    d.text(p + 6, ybot - 66, lw, 60, t, valign='bottom')
    y = by + bh + 110
    d.text(30, ybot - 170 + spec.get('purpose_dy', 0), 300, 160,
           f'<b>Цель:</b> {escape(spec["purpose"])}<br><br><b>Точка зрения:</b> {escape(spec["viewpoint"])}', size=14)
    return d


def save(diagrams, path):
    pages = ''.join(dg.xml(name) for name, dg in diagrams)
    with open(path, 'w', encoding='utf8') as f:
        f.write(f'<mxfile host="drawio" agent="lab3-generator" version="24.0.0">{pages}</mxfile>')

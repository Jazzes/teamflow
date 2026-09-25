import os
from idef0 import build, build_context, save

META = dict(author='Лисичкин Б. О., ВБИо-402рсоб', project='TeamFlow', date='25.09.2026')

# ---------------- A-0
A_0 = dict(node='A-0', title='Управлять задачами команды разработки', number=1, W=1250, H=760,
    box='Управлять задачами команды разработки',
    purpose='показать, как система TeamFlow поддерживает полный цикл работы команды: от требований до отчёта по спринту.',
    viewpoint='руководитель команды разработки (тимлид).',
    arrows=[
        dict(kind='I', code='I1', label='Требования заказчика'),
        dict(kind='I', code='I2', label='Данные о сотрудниках'),
        dict(kind='I', code='I3', label='Сведения о выполненной работе'),
        dict(kind='C', code='C1', label='Регламент Scrum', lw=105),
        dict(kind='C', code='C2', label='Правила жизненного цикла задачи', lw=105),
        dict(kind='C', code='C3', label='Политика доступа', lw=105),
        dict(kind='O', code='O1', label='План спринта'),
        dict(kind='O', code='O2', label='Задачи с актуальным статусом'),
        dict(kind='O', code='O3', label='Отчёты о ходе проекта'),
        dict(kind='M', code='M1', label='ИС TeamFlow (REST API, PostgreSQL)', lw=200, mside='left'),
        dict(kind='M', code='M2', label='Участники команды', lw=200),
    ])

# ---------------- A0
A0 = dict(node='A0', title='Управлять задачами команды разработки', number=2, bw=175, bh=92, gapx=62, gapy=40, left0=215, top0=175, right=215, bottom=200,
    boxes=[dict(id='A1', name='Управлять командами и доступом', num='A1'),
           dict(id='A2', name='Вести проекты и бэклог', num='A2'),
           dict(id='A3', name='Планировать и проводить спринты', num='A3'),
           dict(id='A4', name='Выполнять задачи', num='A4'),
           dict(id='A5', name='Формировать отчётность', num='A5')],
    arrows=[
        dict(frm='I', code='I2', label='Данные о сотрудниках', to=[('A1', 'I')]),
        dict(frm='I', code='I1', label='Требования заказчика', to=[('A2', 'I')]),
        dict(frm='I', code='I3', label='Сведения о выполненной работе', to=[('A4', 'I')], lw=200),
        dict(frm='C', code='C3', label='Политика доступа', to=[('A1', 'C')], lw=110),
        dict(frm='C', code='C1', label='Регламент Scrum', to=[('A3', 'C'), ('A5', 'C')], lw=110),
        dict(frm='C', code='C2', label='Правила жизненного цикла задачи', to=[('A4', 'C')], lw=110),
        dict(frm='A1', label='Учётные записи, состав команд и роли', to=[('A2', 'C'), ('A3', 'C'), ('A4', 'C')], lw=230),
        dict(frm='A2', label='Бэклог проекта', to=[('A3', 'I')], lw=110),
        dict(frm='A3', label='План спринта', code='O1', to=['O']),
        dict(frm='A3', label='Задачи спринта', to=[('A4', 'I')], lw=110),
        dict(frm='A3', label='Незавершённые задачи', to=[('A2', 'I')], lw=220, seg=2),
        dict(frm='A4', label='Задачи с актуальным статусом', code='O2', to=['O'], lw=200),
        dict(frm='A4', label='Статусы задач спринта', to=[('A3', 'I')], lw=220, seg=2, same_port=True),
        dict(frm='A4', label='Журнал работ', to=[('A5', 'I')], lw=100),
        dict(frm='A5', label='Отчёты о ходе проекта', code='O3', to=['O'], lw=200),
        dict(frm='M', code='M1', label='ИС TeamFlow', mside='left', lw=120, to=[('A1', 'M'), ('A2', 'M'), ('A3', 'M'), ('A4', 'M'), ('A5', 'M')]),
        dict(frm='M', code='M2', label='Участники команды', lw=120, to=[('A1', 'M'), ('A2', 'M'), ('A3', 'M'), ('A4', 'M'), ('A5', 'M')]),
    ])

CH = dict(bw=200, bh=100, gapx=75, gapy=45, left0=230, top0=180, right=230, bottom=190)

# ---------------- A1
A1 = dict(node='A1', title='Управлять командами и доступом', number=3, **CH,
    boxes=[dict(id='A11', name='Зарегистрировать пользователя', num='A11'),
           dict(id='A12', name='Сформировать команду', num='A12'),
           dict(id='A13', name='Назначить роли участникам', num='A13')],
    arrows=[
        dict(frm='I', code='I1', label='Данные о сотрудниках', to=[('A11', 'I')]),
        dict(frm='C', code='C1', label='Политика доступа', to=[('A11', 'C'), ('A12', 'C'), ('A13', 'C')], lw=150),
        dict(frm='A11', label='Учётные записи', to=[('A12', 'I'), ('A13', 'I')], lw=160),
        dict(frm='A12', label='Команда', to=[('A13', 'I')], lw=100),
        dict(frm='A13', label='Учётные записи, состав команд и роли', code='O1', to=['O'], lw=230),
        dict(frm='M', code='M1', label='ИС TeamFlow', mside='left', lw=120, to=[('A11', 'M'), ('A12', 'M'), ('A13', 'M')]),
        dict(frm='M', code='M2', label='Участники команды', lw=130, to=[('A11', 'M'), ('A12', 'M'), ('A13', 'M')]),
    ])

# ---------------- A2
A2 = dict(node='A2', title='Вести проекты и бэклог', number=4, **dict(CH, bh=124),
    boxes=[dict(id='A21', name='Создать проект', num='A21'),
           dict(id='A22', name='Настроить метки проекта', num='A22'),
           dict(id='A23', name='Сформировать задачи бэклога', num='A23')],
    arrows=[
        dict(frm='I', code='I1', label='Требования заказчика', to=[('A21', 'I'), ('A23', 'I')]),
        dict(frm='I', code='I2', label='Незавершённые задачи', to=[('A23', 'I')], lw=260),
        dict(frm='C', code='C1', label='Учётные записи, состав команд и роли', to=[('A21', 'C'), ('A23', 'C')], lw=200),
        dict(frm='A21', label='Проект', to=[('A22', 'I'), ('A23', 'I')], lw=90),
        dict(frm='A22', label='Метки проекта', to=[('A23', 'C')], lw=130),
        dict(frm='A23', label='Бэклог проекта', code='O1', to=['O'], lw=200),
        dict(frm='M', code='M1', label='ИС TeamFlow', mside='left', lw=120, to=[('A21', 'M'), ('A22', 'M'), ('A23', 'M')]),
        dict(frm='M', code='M2', label='Участники команды', lw=130, to=[('A21', 'M'), ('A22', 'M'), ('A23', 'M')]),
    ])

# ---------------- A3
A3 = dict(node='A3', title='Планировать и проводить спринты', number=5, **CH,
    boxes=[dict(id='A31', name='Спланировать спринт', num='A31'),
           dict(id='A32', name='Запустить спринт', num='A32'),
           dict(id='A33', name='Закрыть спринт', num='A33')],
    arrows=[
        dict(frm='I', code='I1', label='Бэклог проекта', to=[('A31', 'I')]),
        dict(frm='I', code='I2', label='Статусы задач спринта', to=[('A33', 'I')]),
        dict(frm='C', code='C1', label='Регламент Scrum', to=[('A31', 'C'), ('A32', 'C'), ('A33', 'C')], lw=160, cside='left'),
        dict(frm='C', code='C2', label='Учётные записи, состав команд и роли', to=[('A31', 'C')], lw=180),
        dict(frm='A31', label='План спринта', code='O1', to=['O'], lw=200),
        dict(frm='A31', label='План спринта', to=[('A32', 'I')], nolabel=True, same_port=True),
        dict(frm='A32', label='Задачи спринта', code='O2', to=['O'], lw=200),
        dict(frm='A32', label='Активный спринт', to=[('A33', 'I')], lw=150),
        dict(frm='A33', label='Незавершённые задачи', code='O3', to=['O'], lw=200),
        dict(frm='M', code='M1', label='ИС TeamFlow', mside='left', lw=120, to=[('A31', 'M'), ('A32', 'M'), ('A33', 'M')]),
        dict(frm='M', code='M2', label='Участники команды', lw=130, to=[('A31', 'M'), ('A32', 'M'), ('A33', 'M')]),
    ])

# ---------------- A4
A4 = dict(node='A4', title='Выполнять задачи', number=6, join_dx=30, **dict(CH, bw=185, bh=100, gapx=70, gapy=42, right=340),
    boxes=[dict(id='A41', name='Взять задачу в работу', num='A41'),
           dict(id='A42', name='Обсуждать задачу и прикладывать файлы', num='A42'),
           dict(id='A43', name='Учесть затраченное время', num='A43'),
           dict(id='A44', name='Изменить статус задачи', num='A44')],
    arrows=[
        dict(frm='I', code='I2', label='Задачи спринта', to=[('A41', 'I')]),
        dict(frm='I', code='I1', label='Сведения о выполненной работе', to=[('A43', 'I')], lw=220),
        dict(frm='C', code='C1', label='Правила жизненного цикла задачи', to=[('A41', 'C'), ('A44', 'C')], lw=150, cside='left'),
        dict(frm='C', code='C2', label='Учётные записи, состав команд и роли', to=[('A41', 'C'), ('A42', 'C'), ('A43', 'C')], lw=180),
        dict(frm='A41', label='Задача в работе', to=[('A42', 'I'), ('A43', 'I'), ('A44', 'I')], lw=150),
        dict(frm='A42', label='Комментарии и вложения', to=['O'], lw=230, out_y='J', boundary='<b>O2</b> Журнал работ'),
        dict(frm='A43', label='Записи трудозатрат', to=['O'], lw=200, out_y='J'),
        dict(frm='A44', label='Задачи с актуальным статусом', code='O1', to=['O'], lw=250),
        dict(frm='A44', label='История статусов', to=['O'], lw=200, out_y='J'),
        dict(frm='M', code='M1', label='ИС TeamFlow', mside='left', lw=120, to=[('A41', 'M'), ('A42', 'M'), ('A43', 'M'), ('A44', 'M')]),
        dict(frm='M', code='M2', label='Участники команды', lw=130, to=[('A41', 'M'), ('A42', 'M'), ('A43', 'M'), ('A44', 'M')]),
    ])

# ---------------- A5
A5 = dict(node='A5', title='Формировать отчётность', number=7, **CH,
    boxes=[dict(id='A51', name='Свести трудозатраты по задачам', num='A51'),
           dict(id='A52', name='Проанализировать историю статусов', num='A52'),
           dict(id='A53', name='Подготовить отчёт по спринту', num='A53')],
    arrows=[
        dict(frm='I', code='I1', label='Журнал работ', to=[('A51', 'I'), ('A52', 'I')]),
        dict(frm='C', code='C1', label='Регламент Scrum', to=[('A53', 'C')], lw=150),
        dict(frm='A51', label='Сводка трудозатрат', to=[('A53', 'I')], lw=170),
        dict(frm='A52', label='Метрики потока задач', to=[('A53', 'I')], lw=180),
        dict(frm='A53', label='Отчёты о ходе проекта', code='O1', to=['O'], lw=200),
        dict(frm='M', code='M1', label='ИС TeamFlow', mside='left', lw=120, to=[('A51', 'M'), ('A52', 'M'), ('A53', 'M')]),
        dict(frm='M', code='M2', label='Участники команды', lw=130, to=[('A51', 'M'), ('A52', 'M'), ('A53', 'M')]),
    ])

def fix(spec):
    for a in spec['arrows']:
        a['from'] = a.pop('frm')
    return spec

if __name__ == '__main__':
    pages = [('A-0', build_context(A_0, META)), ('A0', build(fix(A0), META))]
    for sp in (A1, A2, A3, A4, A5):
        pages.append((sp['node'], build(fix(sp), META)))
    save(pages, os.path.join(os.path.dirname(os.path.abspath(__file__)), '..', 'diagrams', 'teamflow-idef0.drawio'))

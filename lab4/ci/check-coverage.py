"""Проверка порога покрытия по отчёту Cobertura: падает, если строк или ветвей покрыто меньше 100%."""
import glob
import sys
import xml.etree.ElementTree as ET

THRESHOLD = 1.0

reports = glob.glob(sys.argv[1] if len(sys.argv) > 1 else "TestResults/**/coverage.cobertura.xml", recursive=True)
if not reports:
    print("Отчёт о покрытии не найден")
    sys.exit(1)

root = ET.parse(reports[0]).getroot()
line_rate = float(root.get("line-rate", 0))
branch_rate = float(root.get("branch-rate", 0))
print(f"Покрытие строк: {line_rate:.2%}, ветвей: {branch_rate:.2%} (порог {THRESHOLD:.0%})")

if line_rate < THRESHOLD or branch_rate < THRESHOLD:
    print("Покрытие ниже порога, сборка отклонена")
    sys.exit(1)

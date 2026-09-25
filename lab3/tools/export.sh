#!/bin/sh
# экспорт каждой страницы .drawio в PNG: export.sh file.drawio prefix count
f=$1; pre=$2; n=$3
i=1
while [ $i -le $n ]; do
  timeout 120 xvfb-run -a drawio --no-sandbox -x -f png -s 1.6 -p $i -o "${pre}-$i.png" "$f" >/dev/null 2>&1
  i=$((i+1))
done

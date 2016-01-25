#!/bin/bash

if [[ $# != 2 ]]; then
	echo "Usage:"
	echo "  $0 input.pptx output.md"
	exit 1
fi

WORK_DIR=`mktemp -d`
IN_FILE=$1
OUT_FILE=$2

unzip "$IN_FILE" -d "$WORK_DIR"

slide=1

while [[ -f "$WORK_DIR/ppt/slides/slide${slide}.xml" ]]; do
	XML="$WORK_DIR/ppt/slides/slide${slide}.xml"

	echo
	echo '<!-- attr: { hasScriptWrapper:true, showInPresentation:true } -->'

	sed -e 's/</\n</g' -e 's/>/>\n/g' "$XML" | while read line; do
		[[ $line == '</a:p>' ]] && printf "\n" && continue
		[[ $line == '<p:sp>' ]] && incode=0 && continue
		[[ $line == '</p:sp>' ]] && [[ $incode == 1 ]] && printf "\`\`\`\n" && continue
		[[ $line == '<p:ph type="body"'* ]] && incode=1 && printf "\`\`\`\n" && continue
		[[ $line == '<p:ph type="title"'* ]] && printf "# " && continue
		[[ $line == '<p:ph type="ctrTitle"'* ]] && printf "# " && continue
		[[ $line == '<p:ph type="subTitle"'* ]] && printf "## " && continue
		[[ $line == '<a:pPr'*'lvl="3"'* ]] && printf "      - " && continue
		[[ $line == '<a:pPr'*'lvl="2"'* ]] && printf "    - " && continue
		[[ $line == '<a:pPr'*'lvl="1"'* ]] && printf "  - " && continue
		[[ $line == '<a:pPr'* ]] && printf -- "- " && continue
		[[ $line == '<'*'>' ]] && continue
		echo -n "$line"
	done

	echo

	let slide+=1
done | tr -d '\r' > "$OUT_FILE"

rm -r "$WORK_DIR"

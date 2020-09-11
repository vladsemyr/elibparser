
let ret = "";
let els = document.getElementsByTagName("font");
for (let i = 0; i < els.length; ++i) {
    if (els[i].textContent.includes("ВСЕГО НАЙДЕНО ПУБЛИКАЦИЙ:"))
    {
        ret = els[i].nextSibling.textContent;
    }
}

ret
let els = document.getElementsByTagName("textarea");
for (let i = 0; i < els.length; ++i) {
    if (els[i].getAttribute("name") == "ftext")
    {
        els[i].value = keyWord;
    }
}


els = document.getElementsByTagName("select");
for (let i = 0; i < els.length; ++i) {
    if (
            (els[i].getAttribute("name") == "begin_year") ||
            (els[i].getAttribute("name") == "end_year")) {
        let ch = els[i].children;
        for (let j = 0; j < ch.length; ++j) {
            ch[j].selected = false;
            if (ch[j].value == publicationYear) {
                ch[j].selected = true;
            }
        }
    }
}


els = document.getElementsByTagName("a");
for (let i = 0; i < els.length; ++i) {
    if (els[i].text == "Поиск") {
        els[i].click();
    }
}

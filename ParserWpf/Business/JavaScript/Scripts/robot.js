els = document.getElementsByTagName("font");
result = false;
for (let i = 0; i < els.length; ++i) {
    if (els[i].text === "Контакты") {
        result = true;
    }
}
result
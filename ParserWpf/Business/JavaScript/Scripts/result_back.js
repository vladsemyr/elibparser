els = document.getElementsByTagName("a");
for (let i = 0; i < els.length; ++i) {
    if (els[i].text === "Вернуться к поисковой форме и изменить условия запроса") {
        els[i].click();
    }
}

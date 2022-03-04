export function goToAnchor(id) {
    var element = document.getElementById(id);
    if (element != null) {
        element.scrollIntoView();
    }
}

export function goToAnchorBottom(id) {
    var element = document.getElementById(id);
    if (element != null) {
        element.scrollIntoView(false);
    }
}

export function collapseFilter(id) {
    new bootstrap.Collapse(document.getElementById(id)).toggle();
}


// Inspired by https://shauncurtis.github.io/posts/DynamicCss.html
export function addCss(url) {
    const elementId = "moongrid_dynamic_css";

    var link = document.getElementById(elementId);

    if (link === undefined || link == null) {
        link = document.createElement("link");
        link.id = elementId;
        document.head.insertBefore(link, document.head.firstChild);
        link.type = 'text/css';
        link.rel = 'stylesheet';
        link.href = url;
    }

    return true;
}

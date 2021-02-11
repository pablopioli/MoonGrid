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
export function goToAnchor(id) {
    document.getElementById(id).scrollIntoView();
}

export function goToAnchorBottom(id) {
    document.getElementById(id).scrollIntoView(false);
}

window.goToAnchor = (id) => {
    document.getElementById(id).scrollIntoView();
};

window.goToAnchorBottom = (id) => {
    document.getElementById(id).scrollIntoView(false);
};

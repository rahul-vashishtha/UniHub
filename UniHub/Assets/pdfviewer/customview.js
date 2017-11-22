//
// NOTE: 
// Modifying the URL below to another server will likely *NOT* work. Because of browser
// security restrictions, we have to use a file server with special headers
// (CORS) - most servers don't support cross-origin browser requests.
//




//
// Disable workers to avoid yet another cross-origin issue (workers need the URL of
// the script to be loaded, and currently do not allow cross-origin scripts)
//
PDFJS.disableWorker = false;

var pdfDoc = null,
    pageNum = 1,
    scale = 1;

//
// Get page info from document, resize canvas accordingly, and render page
//

function renderPage(num, canvas, ctx) {
    // Using promise to fetch the page
    pdfDoc.getPage(num).then(function (page) {
        var viewport = page.getViewport(scale);
        canvas.height = viewport.height;
        canvas.width = viewport.width;

        // Render PDF page into canvas context
        var renderContext = {
            canvasContext: ctx,
            viewport: viewport
        };
        page.render(renderContext);
    });
}

//
// Go to previous page
//
function goPrevious() {
    if (pageNum <= 1)
        return;
    pageNum--;
    renderPage(pageNum);
}

//
// Go to next page
//
function goNext() {
    if (pageNum >= pdfDoc.numPages)
        return;
    pageNum++;
    renderPage(pageNum);
}

//
// Asynchronously download PDF as an ArrayBuffer
//

PDFJS.getDocument(url).then(function getPdfHelloWorld(_pdfDoc) {
    pdfDoc = _pdfDoc;
    var pages = 1;

    while (pages <= pdfDoc.numPages) {
        var newdiv = document.createElement('div');
        var canvas2 = document.createElement('canvas');
        canvas2.id = "the-canvas" + pages;
        newdiv.appendChild(canvas2);
        document.body.appendChild(newdiv);
        var ctx2 = canvas2.getContext('2d');

        renderPage(pages, canvas2, ctx2);
        pages++;
    }

});

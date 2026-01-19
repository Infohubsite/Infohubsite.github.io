async function uploadFileToKoofr(url, streamReference, fileName, contentType) {
    const arrayBuffer = await streamReference.arrayBuffer();
    const blob = new Blob([arrayBuffer], { type: contentType });
    const formData = new FormData();
    formData.append('file', blob, fileName);

    const response = await fetch(url, {
        method: 'POST',
        body: formData
    });

    if (!response.ok) {
        throw new Error(`Upload failed with status: ${response.status}`);
    }
    return await response.json();
}

async function triggerFileDownload(fileName, url) {
    try {
        const response = await fetch(url);
        if (!response.ok) return false;

        const blob = await response.blob();
        const objectUrl = URL.createObjectURL(blob);
        const anchor = document.createElement("a");

        anchor.href = objectUrl;
        anchor.download = fileName;
        document.body.appendChild(anchor);
        anchor.click();

        document.body.removeChild(anchor);
        setTimeout(() => URL.revokeObjectURL(objectUrl), 100);
        return true;
    } catch (error) {
        console.error("Error triggering download:", error);
        return false;
    }
};

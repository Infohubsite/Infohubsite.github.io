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
let stream = null;

export async function startVideo(videoId) {
    const video = document.getElementById(videoId);
    if (!video) throw `Element ${videoId} not found`;
    if (stream) return;
    stream = await navigator.mediaDevices.getUserMedia({
        video: { width: { ideal: 1280 }, height: { ideal: 960 }, facingMode: "user" },
        audio: false
    });
    video.srcObject = stream;
}

export function stopVideo(videoId) {
    const video = document.getElementById(videoId);
    if (stream) {
        stream.getTracks().forEach(t => t.stop());
        stream = null;
    }
    if (video) video.srcObject = null;
}

export function captureToCanvas(videoId, canvasId) {
    const video = document.getElementById(videoId);
    const canvas = document.getElementById(canvasId);
    if (!video || !canvas) throw 'Missing elements';
    canvas.width = video.videoWidth || canvas.width;
    canvas.height = video.videoHeight || canvas.height;
    const ctx = canvas.getContext('2d');
    ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
}

export function imageElementToCanvas(imageId, canvasId) {
    const img = document.getElementById(imageId);
    const canvas = document.getElementById(canvasId);
    if (!img || !canvas) throw 'Missing elements';
    canvas.width = img.naturalWidth;
    canvas.height = img.naturalHeight;
    const ctx = canvas.getContext('2d');
    ctx.drawImage(img, 0, 0);
}

export function getCanvasAsBase64(canvasId) {
    const canvas = document.getElementById(canvasId);
    if (!canvas) throw `Canvas ${canvasId} not found`;
    return canvas.toDataURL('image/png').split(',')[1]; // base64 only
}
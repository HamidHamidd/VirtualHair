// --- Elements ---
const uploadInput = document.getElementById('imageUpload');
const canvas = document.getElementById('previewCanvas');
const ctx = canvas.getContext('2d');
const styleThumbs = document.querySelectorAll('.style-thumb');
const activeLayerIndicator = document.getElementById('activeLayerIndicator');

const saveBtn = document.getElementById("saveLookBtn");
const titleInput = document.getElementById("lookTitle");
const saveMsg = document.getElementById("saveLookMsg");
const resetBtn = document.getElementById("resetOverlay");

let selectedHairstyleId = null;
let selectedFacialHairId = null;

let baseImage = null;
let isImageLoaded = false;
let cropper = null;

/* ---------------- LAYERS ---------------- */

let hairstyleImage = null;
let facialImage = null;

let hairstylePlaced = false;
let facialPlaced = false;

let hairstyleState = { x: 0, y: 0, scale: 0.65, rotation: 0 };
let facialState = { x: 0, y: 0, scale: 0.4, rotation: 0 };

let activeLayer = null; // "hair" or "facial"

const STEP_MOVE = 5;
const STEP_ZOOM = 0.05;
const STEP_ROTATE = 2;

/* ---------------- AI / FACE DETECTION STATE ---------------- */
let detectedFace = null;
let modelsLoaded = false;

// Load Models from CDN
async function loadAIModels() {
    try {
        const MODEL_URL = 'https://justadudewhohacks.github.io/face-api.js/models/';
        // Use SsdMobilenetv1 for higher precision
        await faceapi.nets.ssdMobilenetv1.loadFromUri(MODEL_URL);
        await faceapi.nets.faceLandmark68Net.loadFromUri(MODEL_URL);
        modelsLoaded = true;
        console.log("AI Models Loaded Successfully");
    } catch (e) {
        console.error("AI Models failed to load", e);
    }
}
loadAIModels();

async function detectFace() {
    if (!modelsLoaded || !baseImage) return;
    
    const status = document.getElementById('aiDetectorStatus');
    const success = document.getElementById('aiSuccessStatus');
    
    if (status) status.classList.remove('d-none');
    if (success) success.classList.add('d-none');

    try {
        // High quality detection
        const detection = await faceapi.detectSingleFace(canvas).withFaceLandmarks();
        
        if (detection) {
            detectedFace = detection;
            console.log("AI Detected Face:", detectedFace);
            
            // Show success badge for 3 seconds
            if (success) {
                success.classList.remove('d-none');
                setTimeout(() => success.classList.add('d-none'), 3000);
            }
        } else {
            console.warn("AI could not find a face.");
        }
    } catch (e) {
        console.error("Detection error:", e);
    } finally {
        if (status) status.classList.add('d-none');
    }
}

document.getElementById('reScanFace')?.addEventListener('click', () => {
    detectFace();
});

/* ---------------- CROP MODAL ---------------- */

const cropModal = document.createElement('div');
cropModal.className = 'cropper-modal-overlay';
cropModal.style.cssText = 'display: none; position: fixed; inset: 0; background: rgba(0,0,0,0.9); backdrop-filter: blur(10px); z-index: 1050; align-items: center; justify-content: center;';
cropModal.innerHTML = `
<div class="bg-surface border border-secondary border-opacity-25" style="width: 90%; max-width: 600px; padding: 3rem;">
  <h4 class="brand-font text-white mb-2 text-center">Adjust Photo</h4>
  <p class="text-secondary small mb-4 text-center text-uppercase letter-spacing-1">Center your face for best results</p>
  <div class="cropper-img-container bg-black mb-4 position-relative overflow-hidden border border-secondary border-opacity-25" style="max-height: 50vh;">
    <img id="cropperImage" style="max-width:100%; display:block;">
  </div>
  <div class="d-flex gap-3">
    <button id="cancelCrop" class="btn btn-ghost flex-grow-1 py-3">Cancel</button>
    <button id="confirmCrop" class="btn btn-luxury flex-grow-1 py-3">Confirm</button>
  </div>
</div>`;
document.body.appendChild(cropModal);

/* ---------------- UPLOAD ---------------- */

uploadInput?.addEventListener('change', (e) => {
    const file = e.target.files[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (event) => {
        const cropperImg = document.getElementById('cropperImage');
        if (cropperImg) {
            cropperImg.src = event.target.result;
            if (cropModal) cropModal.style.display = 'flex';

            cropperImg.onload = () => {
                if (cropper) cropper.destroy();
                cropper = new Cropper(cropperImg, {
                    aspectRatio: 1,
                    viewMode: 1,
                    dragMode: 'move',
                    movable: true,
                    zoomable: true,
                    background: false,
                    guides: false,
                    center: true,
                    highlight: false,
                    cropBoxMovable: true,
                    cropBoxResizable: true,
                    toggleDragModeOnDblclick: false
                });
            };
        }
    };
    reader.readAsDataURL(file);
});

document.getElementById('confirmCrop')?.addEventListener('click', () => {
    if (!cropper) return;

    const croppedCanvas = cropper.getCroppedCanvas({ width: 500, height: 500 });
    if (!croppedCanvas) return;

    baseImage = new Image();
    baseImage.onload = () => {
        isImageLoaded = true;
        maskCanvas.width = 500;
        maskCanvas.height = 500;
        maskCtx.clearRect(0, 0, maskCanvas.width, maskCanvas.height);
        if (cropModal) cropModal.style.display = 'none';
        drawCanvas();
        detectFace(); // Trigger AI analysis
    };
    baseImage.src = croppedCanvas.toDataURL('image/png');
});

document.getElementById('cancelCrop')?.addEventListener('click', () => {
    if (cropModal) cropModal.style.display = 'none';
    if (cropper) cropper.destroy();
});

/* ---------------- THUMBNAILS ---------------- */

let selectedHairThumb = null;
let selectedFacialThumb = null;

let selectedHairstyleName = "";
let selectedFacialHairName = "";

function selectNone(type) {
    if (type === 'hairstyle') {
        if (selectedHairThumb) selectedHairThumb.classList.remove("selected-thumb");
        selectedHairstyleId = null;
        selectedHairstyleName = "";
        selectedHairThumb = null;
        activeLayer = "hair";
    } else {
        if (selectedFacialThumb) selectedFacialThumb.classList.remove("selected-thumb");
        selectedFacialHairId = null;
        selectedFacialHairName = "";
        selectedFacialThumb = null;
        activeLayer = "facial";
    }
    updateActiveUI();
}

styleThumbs.forEach((thumb) => {
    thumb.addEventListener('click', () => {
        if (!isImageLoaded) {
            alert("Please upload and crop your photo first!");
            return;
        }

        const type = thumb.dataset.type;
        const id = parseInt(thumb.dataset.id);

        if (type === "hairstyle") {
            if (selectedHairThumb) selectedHairThumb.classList.remove("selected-thumb");
            selectedHairThumb = thumb.closest('.style-thumb-container').querySelector('.style-thumb');
            selectedHairThumb.classList.add("selected-thumb");
            activeLayer = "hair";
            selectedHairstyleId = id;
            selectedHairstyleName = thumb.nextElementSibling?.textContent?.trim() || "modern hairstyle";
        }

        if (type === "facial") {
            if (selectedFacialThumb) selectedFacialThumb.classList.remove("selected-thumb");
            selectedFacialThumb = thumb.closest('.style-thumb-container').querySelector('.style-thumb');
            selectedFacialThumb.classList.add("selected-thumb");
            activeLayer = "facial";
            selectedFacialHairId = id;
            selectedFacialHairName = thumb.nextElementSibling?.textContent?.trim() || "modern beard";
        }
        
        updateActiveUI();
    });
});

function updateActiveUI() {
    // Basic indicator logic omitted for brevity in AI mode
}

/* ---------------- DRAW ---------------- */

let aiGeneratedImage = null;

function drawCanvas() {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    
    // In AI mode, we just show the base image, or the returned AI image.
    if (aiGeneratedImage) {
        ctx.drawImage(aiGeneratedImage, 0, 0, canvas.width, canvas.height);
    } else if (baseImage) {
        ctx.drawImage(baseImage, 0, 0, canvas.width, canvas.height);
    }
}

/* ---------------- AI GENERATOR ---------------- */

function generateAiMask(type) {
    const aiMaskCanvas = document.createElement('canvas');
    aiMaskCanvas.width = canvas.width;
    aiMaskCanvas.height = canvas.height;
    const maskCtx = aiMaskCanvas.getContext('2d');
    
    maskCtx.fillStyle = "black";
    maskCtx.fillRect(0, 0, aiMaskCanvas.width, aiMaskCanvas.height);
    
    if (detectedFace) {
        maskCtx.fillStyle = "white";
        const landmarks = detectedFace.landmarks.positions;
        const faceWidth = Math.abs(landmarks[16].x - landmarks[0].x);
        
        if (type === "hair") {
            const topY = landmarks[19].y;
            maskCtx.beginPath();
            maskCtx.moveTo(landmarks[0].x - 20, landmarks[0].y);
            maskCtx.bezierCurveTo(
                landmarks[0].x - 60, landmarks[0].y - faceWidth * 1.5,
                landmarks[16].x + 60, landmarks[16].y - faceWidth * 1.5,
                landmarks[16].x + 20, landmarks[16].y
            );
            maskCtx.lineTo(landmarks[16].x + 20, topY);
            maskCtx.lineTo(landmarks[0].x - 20, topY);
            maskCtx.closePath();
            maskCtx.fill();
        } else if (type === "facial") {
             const chin = landmarks[8];
             const mouth = landmarks[33];
             maskCtx.beginPath();
             // Simple beard region mask
             maskCtx.arc(chin.x, mouth.y, faceWidth * 0.65, 0, Math.PI * 2);
             maskCtx.fill();
        }
    } else {
        // Fallback: full white mask if no face detected
        maskCtx.fillStyle = "white";
        maskCtx.fillRect(0, 0, aiMaskCanvas.width, aiMaskCanvas.height);
    }
    
    return aiMaskCanvas.toDataURL("image/png");
}

const generateAiBtn = document.getElementById("generateAiBtn");
const aiLoadingState = document.getElementById("aiLoadingState");
const originalCanvasWrapper = canvas.parentElement;

generateAiBtn?.addEventListener("click", async () => {
    if (!isImageLoaded || !baseImage) {
        alert("Upload and crop a photo first!");
        return;
    }

    if (!selectedHairstyleId && !selectedFacialHairId) {
        alert("Please select a hairstyle or facial hair style to generate.");
        return;
    }

    const type = selectedHairstyleId ? "hair" : "facial";
    const prompt = selectedHairstyleId ? selectedHairstyleName : selectedFacialHairName;
    const maskBase64 = generateAiMask(type);

    // Get original image base64 directly from canvas currently
    const originalCanvas = document.createElement('canvas');
    originalCanvas.width = canvas.width;
    originalCanvas.height = canvas.height;
    originalCanvas.getContext('2d').drawImage(baseImage, 0, 0);
    const originalBase64 = originalCanvas.toDataURL("image/png");

    generateAiBtn.disabled = true;
    generateAiBtn.innerHTML = '<i class="fa-solid fa-spinner fa-spin me-2"></i> Generating...';
    aiLoadingState.classList.remove('d-none');
    canvas.style.opacity = '0.3'; // dim canvas during process

    try {
        const res = await fetch("/api/ai/hair-edit", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                OriginalImageBase64: originalBase64,
                MaskImageBase64: maskBase64,
                Prompt: prompt,
                Type: type
            })
        });

        const data = await res.json();
        if (data.success && data.imageUrl) {
            // Success! Load the newly generated image
            const newImg = new Image();
            newImg.onload = () => {
                aiGeneratedImage = newImg;
                drawCanvas();
                
                // Show Save button and clean up UI
                const saveLookBtn = document.getElementById('saveLookBtn');
                if (saveLookBtn) saveLookBtn.classList.remove('d-none');
            };
            newImg.src = data.imageUrl;
        } else {
            alert("AI Generation failed: " + (data.message || "Unknown error"));
        }
    } catch (e) {
        console.error("AI Generation request failed", e);
        alert("An error occurred trying to generate the image.");
    } finally {
        generateAiBtn.disabled = false;
        generateAiBtn.innerHTML = '<i class="fa-solid fa-wand-magic-sparkles me-2"></i> Generate Again';
        aiLoadingState.classList.add('d-none');
        canvas.style.opacity = '1';
    }
});

/* ---------------- SAVE TO GALLERY ---------------- */

function getAntiForgeryToken() {
    const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : "";
}

const saveLookBtn = document.getElementById('saveLookBtn');

saveLookBtn?.addEventListener("click", async () => {
    if (!aiGeneratedImage && !isImageLoaded) {
        alert("Generate an image first!");
        return;
    }

    const title = (document.getElementById("lookTitle").value || "").trim();
    if (!title) {
        alert("Please enter a name for your look.");
        return;
    }

    saveLookBtn.disabled = true;
    saveLookBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';

    const imageData = canvas.toDataURL("image/png");

    try {
        const res = await fetch("/UserHairstyles/SaveLook", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "RequestVerificationToken": getAntiForgeryToken()
            },
            body: JSON.stringify({
                title,
                imageData,
                hairstyleId: selectedHairstyleId,
                facialHairId: selectedFacialHairId
            })
        });

        const data = await res.json();

        if (data.success) {
            window.location.href = "/UserHairstyles";
        } else {
            alert(data.message || "Error saving.");
            saveLookBtn.disabled = false;
            saveLookBtn.innerHTML = '<i class="fa-solid fa-download"></i>';
        }
    } catch (e) {
        alert("An error occurred while saving.");
        saveLookBtn.disabled = false;
        saveLookBtn.innerHTML = '<i class="fa-solid fa-download"></i>';
    }
});

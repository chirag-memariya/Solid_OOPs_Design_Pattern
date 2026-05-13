import * as signalR from "@microsoft/signalr";

// Replace with your actual SignalR hub URL
const hubUrl = "http://127.0.0.1:5050/bridgeUtility";

// Create connection
const connection = new signalR.HubConnectionBuilder().withUrl(hubUrl).build();

// Track connection state
let connectionPromise = null;
let isConnected = false;

// Start connection once and reuse
async function ensureConnectionStarted() {
    if (!connectionPromise) {
        connectionPromise = connection
            .start()
            .then(() => {
                isConnected = true;
                console.log("Connected to SignalR hub.");
            })
            .catch((err) => {
                isConnected = false;
                console.error("Connection error:", err);
                connectionPromise = null;
                throw err;
            });
    }
    return connectionPromise;
}

// Function to start the live stream
async function startLiveStream(zeroBasedRSindex, oneBasedCameraIndex, id) {
    try {
        console.log("Enter", id);

        await ensureConnectionStarted();

        // Call the streaming hub method
        const stream = connection.stream(
            "StartLiveStream",
            zeroBasedRSindex,
            oneBasedCameraIndex
        );

        // Listen for streaming data
        stream.subscribe({
            next: (frame) => {
                // frame is a cMediaFrame object
                console.log(id);
                console.log("Received frame:", frame);
                // , ": Received frame:", frame);
                // Handle/display the frame as needed
            },
            complete: () => {
                console.log("Stream completed.");
            },
            error: (err) => {
                console.error("Stream error:", err);
            },
        });
    } catch (err) {
        console.error("Connection error:", err);
    }
}

// Example usage:
startLiveStream(1, 2, "One"); // Pass your required indices here
setTimeout(() => {
    startLiveStream(1, 2, "Two");
}, 0);
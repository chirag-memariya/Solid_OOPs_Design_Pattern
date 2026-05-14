#!/bin/bash

# Variables
base_url="http://172.16.1.58:32001/api/FaceDetection"
concurrency=10  # Number of multiple requests to perform at a time
total_requests=200     # Total number of requests to perform for the benchmarking session

# Function to perform load testing with curl
perform_load_test() {
    i=1
    while [ $i -le $total_requests ]; do
        camera_id=$((i % 10 + 1))  # Change the range based on the number of cameras

        endpoint="$base_url?cameraId=$camera_id"
        curl -s -o /dev/null -w "Camera ID: $camera_id - HTTP Code: %{http_code}\n" $endpoint &
        
        i=$((i + 1))

        # Add a delay of 100 milliseconds between requests
        sleep 0.1
    done

    # Wait for all background processes to finish
    wait
}

# Perform load testing
perform_load_test

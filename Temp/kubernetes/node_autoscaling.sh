#!/bin/bash
total_worker_node=1
worker_nodes_list=("node1 172.16.1.58 sharanphadke sharan")
# array use all index-1
top=0
bottom=0
current_node_index=0
current_node_ip="172.16.1.58"
current_node_ssh_user="sharanphadke"
current_node_ssh_password="sharan"
scale_up_threshold_cpu=20
scale_up_threshold_memory=20
scale_down_threshold_cpu=60
scale_down_threshold_memory=90


get_current_node_name() {
    node_name="${node_name_list[$current_node_index]}"
    echo $node_name
}

get_current_node_cpu_load() {
    load_cpu="${cpu_percentage_list[$current_node_index]}"
    load_cpu="${load_cpu%\%}"
    echo $load_cpu
}

get_current_node_memory_load() {
    load_memory="${memory_percentage_list[$current_node_index]}"
    load_memory="${load_memory%\%}"
    echo $load_memory
}

join_node() {
    ip=$1
    username=$2
    password=$3
    ssh "$username"@"$ip" "echo '$password' | sudo -S kubeadm join 172.16.1.246:6443 --token 7qbw1l.jxiah9cn83slib5z --discovery-token-ca-cert-hash sha256:21ac008941c639fb8783cfe4da97528ce1f26a7c525e51154424978f63ab439d"
}

remove_node() {
    node=$1
    control_plane=$(kubectl get node $node -o jsonpath='{.metadata.labels.kubernetes\.io\/role}')

    if [ "$control_plane" == "master" ] || [ "$control_plane" == "control-plane" ]; then
        echo "Node $node has control plane role, skipping removal."
    else
        kubectl cordon $node
        kubectl drain $node --force --delete-emptydir-data --ignore-daemonsets
        sleep 60
        kubectl delete node $node
        sshpass -p "$current_node_ssh_password" ssh "$current_node_ssh_user"@"$current_node_ip" "echo '$current_node_ssh_password' | sudo -S kubeadm reset <<< 'yes'"
        echo "Node $node was removed!!!"
    fi
}

kubectl_output=$(kubectl top node)
node_name_list=($(echo "$kubectl_output" | awk 'NR>1 {print $1}'))
cpu_percentage_list=($(echo "$kubectl_output" | awk 'NR>1 {print $3}'))
memory_percentage_list=($(echo "$kubectl_output" | awk 'NR>1 {print $5}'))

while true; do
    echo "start..."
    load_cpu_value=$(get_current_node_cpu_load)
    load_memory_value=$(get_current_node_memory_load)

    if [[ $load_cpu_value -gt $scale_up_threshold_cpu || $load_memory_value -gt $scale_up_threshold_memory ]]; then
        if [ $top -eq $(($total_worker_node-1)) ]; then
            echo "No more nodes to join!!"
            continue
        fi

        top_node_property="${worker_nodes_list[$top]}"
        top_node_name=$(echo "$top_node_property" | awk '{print $1}')

        if ! kubectl get nodes | grep -q "$top_node_name"; then
            echo "Node $top_node_name is not joined yet, joining..."
            node_ip=$(echo "$top_node_property" | awk '{print $2}')
            ssh_username=$(echo "$top_node_property" | awk '{print $3}')
            top_node_ssh_password=$(echo "$top_node_property" | awk '{print $4}')
            join_node "$node_ip" "$ssh_username" "$top_node_ssh_password"
            ((current_node_index++))
            current_node_ip=$node_ip
            current_node_ssh_user=$ssh_username
            current_node_ssh_password=$top_node_ssh_password
            echo "Node $top_node_name was joined successfully!!!"
            sleep 30
            continue
        fi

    elif [[ $load_cpu_value -lt $scale_down_threshold_cpu && $load_memory_value -lt $scale_down_threshold_memory ]]; then
        if [ "$current_node_index" -eq "$top" ]; then
            echo "No nodes to remove. At least one worker node should remain."
            continue
        fi
        
        node_name=$(get_current_node_name)
        echo "Node $node_name cpu and memory loads are low, removing node..."
        if kubectl get nodes | grep -q "$node_name"; then
            echo "Node $node_name is present, removing..."
            remove_node "$node_name"
            ((current_node_index--))
        fi

    fi
    echo "going to sleep..."
    sleep 300
done

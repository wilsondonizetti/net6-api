pipeline {
    agent {
        docker {
            image 'hashicorp/terraform:latest'
            label 'LINUX-SLAVE'
            args  '--entrypoint="" -u root -v /opt/jenkins/.gcp:/root/.gcp'
        }
    }
    options {
        ansiColor('xterm')
    }
    parameters {
        choice(
            choices: ['preview' , 'apply' , 'show', 'preview-destroy' , 'destroy'],
            description: 'Terraform action to apply',
            name: 'action')
        string(defaultValue: "default", description: 'Which GCP Account (Boto profile) do you want to target?', name: 'GCP_PROFILE')
    }
    stages {
        stage('init') {
            steps {
                sh 'terraform version'
                sh 'terraform init -backend-config="bucket=${ACCOUNT}-tfstate" -backend-config="key=${TF_VAR_stack_name}/terraform.tfstate" -backend-config="region=southamerica-a"'
            }
        }
        stage('validate') {
            when {
                expression { params.action == 'preview' || params.action == 'apply' || params.action == 'destroy' }
            }
            steps {
                sh 'terraform validate -var gcp_profile=${GCP_PROFILE}'
            }
        }
        stage('preview') {
            when {
                expression { params.action == 'preview' }
            }
            steps {
                sh 'terraform plan -var gcp_profile=${GCP_PROFILE}'
            }
        }
        stage('apply') {
            when {
                expression { params.action == 'apply' }
            }
            steps {
                sh 'terraform plan -out=plan -var gcp_profile=${GCP_PROFILE}'
                sh 'terraform apply -auto-approve plan'
            }
        }
        stage('show') {
            when {
                expression { params.action == 'show' }
            }
            steps {
                sh 'terraform show'
            }
        }
        stage('preview-destroy') {
            when {
                expression { params.action == 'preview-destroy' }
            }
            steps {
                sh 'terraform plan -destroy -var gcp_profile=${GCP_PROFILE}'
            }
        }
        stage('destroy') {
            when {
                expression { params.action == 'destroy' }
            }
            steps {
                sh 'terraform destroy -force -var gcp_profile=${GCP_PROFILE}'
            }
        }
    }
}

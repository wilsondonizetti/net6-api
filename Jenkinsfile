pipeline {
  agent {
    docker {
      image 'hashicorp/terraform:latest'
    }

  }
  stages {
    stage('Build') {
      steps {
        sh 'docker --version'
      }
    }

  }
}
pipeline {
    agent any
    stages {
        stage('Verify git commit conventions') {
            steps {
                sh 'sh ./gitlint.sh'
            }
        }
        stage('Restore') {
            steps {
                sh 'docker build --target restore .'
            }
        }
        stage('Build') {
            steps {
                sh 'docker build --target build .'
            }
        }
        stage('Test') {
            steps {
                sh 'docker build --target test .'
            }
        }
    }
}

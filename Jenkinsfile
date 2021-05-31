pipeline {
    agent any
    stages {
        stage('Verify git commit conventions') {
            steps {
                sh 'sh ./gitlint.sh'
            }
        }
    }
}

pipeline {
    agent any
    
    parameters {
        string(name: 'BUILD_VERSION', defaultValue: '1.0.0', description: 'Version number')
        choice(name: 'BUILD_CONFIG', choices: ['Release', 'Debug'], description: 'Build configuration')
    }
    
    environment {
        UNITY_PATH = "C:\\Program Files\\Unity\\Hub\\Editor\\2022.3.4f1\\Editor\\Unity.exe"
        REPO_URL = "https://github.com/IvanMoreno/JenkinsTest.git"
    }
    
    stages {
        stage('Checkout') {
            steps {
                cleanWs()
                bat "git clone ${REPO_URL} ."
            }
        }
        
        stage('Run Tests') {
            steps {
                bat """
                    echo "Running Unity tests..."
                    cd "%WORKSPACE%"
                    
                    if not exist "CI" mkdir "CI"
                    
                    "${UNITY_PATH}" -runTests -projectPath "%WORKSPACE%" -exit -batchmode -testResults "%WORKSPACE%\\CI\\results.xml" -testPlatform EditMode
                """
            }
        }
        
        stage('Test No publish') {
            steps {
                bat """
                    if not exist "CI" mkdir "CI"
                    "${UNITY_PATH}" -runTests -projectPath "%WORKSPACE%" -exit -batchmode -testResults "%WORKSPACE%\\CI\\results.xml" -testPlatform EditMode
                """
            }
        }
        
        stage('Test') {
            steps {
                bat """
                    if not exist "CI" mkdir "CI"
                    "${UNITY_PATH}" -runTests -projectPath "%WORKSPACE%" -exit -batchmode -testResults "%WORKSPACE%\\CI\\results.xml" -testPlatform EditMode
                """
                publishTestResults testResultsPattern: "${WORKSPACE}/CI/results.xml"
            }
        }
        
        stage('Build') {
            steps {
                bat """
                    "${UNITY_PATH}" -executeMethod SimpleBuildScript.Build -projectPath "%WORKSPACE%" -quit -batchmode -version "${params.BUILD_VERSION}" -config "${params.BUILD_CONFIG}"
                """
                archiveArtifacts artifacts: 'Build/**/*', fingerprint: true
            }
        }
    }
}
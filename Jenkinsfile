pipeline {
    agent any
    
    environment {
        UNITY_PATH = "C:\\Program Files\\Unity\\Hub\\Editor\\2022.3.4f1\\Editor\\Unity.exe"
        repo = "https://github.com/IvanMoreno/JenkinsTest.git"
        branch = "main"
        workingDir = "${WORKSPACE}"
    }
    
    stages {
        stage('Clean Workspace') {
            steps {
                cleanWs()
            }
        }
        stage('Clone Repository') {
            steps {
                bat """
                    echo "Cloning repository..."
                    git clone --branch ${branch} --depth 1 ${repo} "${workingDir}"
                """
            }
        }
        
        stage('Run Tests') {
            steps {
                bat """
                    echo "Running Unity tests..."
                    cd "${workingDir}"
                    
                    if not exist "CI" mkdir "CI"
                    
                    "${UNITY_PATH}" -runTests -projectPath "${workingDir}" -exit -batchmode -testResults "${workingDir}\\CI\\results.xml" -testPlatform EditMode
                """
            }
        }
        
        stage('Build Windows Application') {
            steps {
                bat """
                    echo "Building Windows application..."
                    cd "${workingDir}"
                    
                    if not exist "CI" mkdir "CI"
                    
                    "${UNITY_PATH}" -executeMethod BuildScript.BuildWindows -projectPath "${workingDir}" -quit -batchmode -buildVersion "${params.BUILD_VERSION}" -buildConfig "${params.BUILD_CONFIGURATION}" -outputPath "${params.OUTPUT_PATH}" -companyName "${params.COMPANY_NAME}" -productName "${params.PRODUCT_NAME}" -logFile "${workingDir}\\CI\\build.log"
                """
            }
        }
    }
}
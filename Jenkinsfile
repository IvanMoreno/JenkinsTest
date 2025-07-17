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
    }
}
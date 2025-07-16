pipeline {
    agent any
    
    environment {
        UNITY_PATH = "C:\\Program Files\\Unity\\Hub\\Editor\\2022.3.4f1\\Editor\\Unity.exe"
        repo = "https://github.com/IvanMoreno/ChristmasLearning.git"
        branch = "master"
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
                    git clone --branch ${branch} --depth 1 ${repo} "${workingDir}\\${branch}"
                """
            }
        }
        
        stage('Run Tests') {
            steps {
                bat """
                    echo "Running Unity tests..."
                    cd "${workingDir}\\${branch}"
                    
                    if not exist "CI" mkdir "CI"
                    
                    "${UNITY_PATH}" -batchmode -projectPath "${workingDir}\\${branch}" -runTests -testResults "${workingDir}\\${branch}\\CI\\results.xml" -testPlatform EditMode -nographics -quit
                """
            }
        }
    }
}
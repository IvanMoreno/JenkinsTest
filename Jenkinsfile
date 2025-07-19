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
        
        stage('Build') {
            steps {
                bat """
                    echo "=== CHECKING FOR BUILD SCRIPT ==="
                    if exist "%WORKSPACE%\\Assets\\Editor\\SimpleBuildScript.cs" (
                        echo "✓ SimpleBuildScript.cs found"
                        type "%WORKSPACE%\\Assets\\Editor\\SimpleBuildScript.cs"
                    ) else (
                        echo "✗ SimpleBuildScript.cs NOT found"
                        echo "Looking for any build scripts:"
                        dir "%WORKSPACE%\\Assets\\Editor\\*.cs"
                    )
                    
                    echo "=== RUNNING UNITY BUILD ==="
                    "${UNITY_PATH}" -executeMethod SimpleBuildScript.Build -projectPath "%WORKSPACE%" -quit -batchmode -version "${params.BUILD_VERSION}" -config "${params.BUILD_CONFIG}" -logFile "%WORKSPACE%\\CI\\build.log"
                    
                    echo "=== BUILD LOG ==="
                    if exist "%WORKSPACE%\\CI\\build.log" (
                        type "%WORKSPACE%\\CI\\build.log"
                    ) else (
                        echo "No build log found"
                    )
                    
                    echo "=== CHECKING BUILD OUTPUT ==="
                    if exist "%WORKSPACE%\\Build" (
                        echo "Build directory found:"
                        dir "%WORKSPACE%\\Build" /s
                    ) else (
                        echo "Build directory NOT found"
                    )
                """
                
                archiveArtifacts artifacts: '**/*', fingerprint: true, allowEmptyArchive: true
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
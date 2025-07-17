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
        
        stage('Build Windows Application') {
            steps {
                bat """
                    echo "Building to Jenkins workspace..."
                    echo "Jenkins Workspace: %WORKSPACE%"
                    echo "Working Directory: ${workingDir}"
                    echo "Build will be placed in: %WORKSPACE%\\Builds"
                    
                    cd "${workingDir}"
                    
                    "${UNITY_PATH}" -executeMethod BuildScript.BuildWindows -projectPath "${workingDir}" -quit -batchmode -buildVersion "${params.BUILD_VERSION}" -buildConfig "${params.BUILD_CONFIGURATION}" -outputPath "%WORKSPACE%\\Builds" -companyName "${params.COMPANY_NAME}" -productName "${params.PRODUCT_NAME}" -logFile "${workingDir}\\CI\\build.log"
                    
                    echo "=== BUILD LOG ==="
                    if exist "${workingDir}\\CI\\build.log" type "${workingDir}\\CI\\build.log"
                    
                    echo "=== CHECKING BUILD OUTPUT ==="
                    set "buildFolder=%WORKSPACE%\\Builds\\${params.PRODUCT_NAME}_${params.BUILD_VERSION}_${params.BUILD_CONFIGURATION}"
                    set "expectedExe=%buildFolder%\\${params.PRODUCT_NAME}.exe"
                    
                    if exist "%expectedExe%" (
                        echo "✓ Build successful!"
                        echo "Build location: %buildFolder%"
                        echo "Executable: %expectedExe%"
                        echo ""
                        echo "Build contents:"
                        dir "%buildFolder%"
                        echo ""
                        echo "Build info:"
                        if exist "%buildFolder%\\BUILD_INFO.txt" type "%buildFolder%\\BUILD_INFO.txt"
                    ) else (
                        echo "✗ Build failed - executable not found at %expectedExe%"
                        echo "Checking workspace builds directory:"
                        if exist "%WORKSPACE%\\Builds" (
                            dir "%WORKSPACE%\\Builds" /s
                        ) else (
                            echo "Builds directory doesn't exist"
                        )
                    )
                """
            }
        }
        
        stage('Archive Build Artifacts') {
            steps {
                script {
                    // Archive the build output
                    def buildFolder = "Builds/${params.PRODUCT_NAME}_${params.BUILD_VERSION}_${params.BUILD_CONFIGURATION}"
                    if (fileExists(buildFolder)) {
                        archiveArtifacts artifacts: "${buildFolder}/**/*", fingerprint: true
                        echo "Build artifacts archived successfully!"
                    } else {
                        echo "No build artifacts to archive"
                    }
                }
            }
        }
    }
}
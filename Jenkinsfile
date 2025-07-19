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
                dir("${workingDir}") {
                    bat "mkdir CI 2>nul || echo CI directory ready"
                    bat "\"${UNITY_PATH}\" -runTests -projectPath \"%CD%\" -exit -batchmode -testResults \"%CD%\\CI\\results.xml\" -testPlatform EditMode -nographics"
                }
                
                script {
                    def results = readFile("${workingDir}/CI/results.xml")
                    def failures = (results =~ /failed="(\d+)"/)[0][1] as Integer
                    def errors = (results =~ /errors="(\d+)"/)[0][1] as Integer
                    
                    if (failures > 0 || errors > 0) {
                        error("Tests failed: ${failures} failures, ${errors} errors")
                    }
                }
                
                publishTestResults testResultsPattern: 'CI/results.xml'
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
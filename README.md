# WinSpeechToText

## Introduction

WinSpeechToText is an application designed to convert spoken language into written text. Leveraging advanced speech recognition technologies, this application provides an efficient and accurate way to transcribe audio input into text format.

## Building the Production-Ready Application

Follow these steps to build the WinSpeechToText application. These instructions are designed to be clear for non-programmer users.

### Prerequisites

1. **Install Visual Studio:**
   - Download and install Visual Studio from [here](https://visualstudio.microsoft.com/downloads/).
   - During installation, select the ".NET desktop development" workload.

2. **Clone the Repository:**
   - Download the repository as a ZIP file and extract it, or use Git to clone the repository:
     
### Building the Application

1. **Open the Solution:**
   - Open Visual Studio.
   - Click on `File` > `Open` > `Project/Solution`.
   - Navigate to the folder where you extracted/cloned the repository and select `WinSpeechToText.sln`.

2. **Restore NuGet Packages:**
   - In Visual Studio, go to `Tools` > `NuGet Package Manager` > `Manage NuGet Packages for Solution`.
   - Click on the `Restore` button to restore all the necessary packages.

3. **Build the Solution:**
   - In Visual Studio, select `Build` > `Build Solution`.
   - Ensure there are no errors in the build process.

### Running the Application

1. **Run the Application:**
   - Press `F5` or click on the `Start` button in Visual Studio to run the application.
   - The application should start, and you should see the main window.

### Creating an Installer (Optional)

1. **Publish the Application:**
   - In Visual Studio, right-click on the project in the Solution Explorer and select `Publish`.
   - Follow the prompts to publish the application. You can choose to publish it as a self-contained application so that it includes all necessary dependencies.

2. **Distribute the Application:**
   - Once published, you can distribute the application by sharing the published files or creating an installer using tools like Inno Setup or WiX.

### Setting Up the API Key

1. **Set Your OpenAI API Key:**
   - Run the application.
   - Click on the `Set API Key` button.
   - Enter your OpenAI API key and save it.

You have successfully built and run the WinSpeechToText application!
# ByteQuiz

An educational Android quiz game project

## Prerequisites

- Git installed on your local machine. You can download Git [here](https://git-scm.com/downloads).
- Unity Hub and Editor on your local machine. You can download Unity Hub [here](https://unity.com/download).

## Getting Started

Follow these steps to set up and run the project on your local machine.

### Cloning the Repository

1. Open a terminal/command prompt.
2. Navigate to the directory where you want to store the project.
3. Run the following command to clone the repository:
```
git clone <repository_url>
```
Replace `<repository_url>` with the URL of your Git repository.

### Setting Up the Project

1. Add the project by going to:
```
Unity Hub > Add > Add Project from Disk > <Select the directory>
```
2. After opening the project change the build platform to Android:
```
File > Build Settings > Platform (Android) > Switch Platform
```

### Running the Project

1. Don't forget to open a scene before running the game.

## Contributing

If you'd like to contribute to the project, please follow these steps:
1. Clone the repository to your local machine using the following command:
```
git clone <repository_url>
```
Replace `<repository_url>` with the URL of the Git repository.

2. Create a new branch with a descriptive name for your feature or bug fix:
```
git checkout -b <branch_name>
```
Replace `<branch_name>` with the name of your new branch.

3. Make your changes to the project files.
4. Stage your changes for commit:
```
git add .
```
This command stages all changes. You can also specify individual files to stage.

5. Commit your changes:
```
git commit -m "Brief description of your changes"
```
6. Push your changes to the remote repository:
```
git push origin <branch_name>
```
Replace `<branch_name>` with the name of your branch.

8. Open a pull request on the original repository through the GitHub website.
9. Wait for your changes to be reviewed and merged.
10. Sync your local repository with the remote repository:
```
git checkout main
git pull origin main
```
Replace `main` with the name of your main branch if it's different.

10. Delete the feature branch:
 ```
 git branch -d <branch_name>
 ```
Replace `<branch_name>` with the name of your branch.

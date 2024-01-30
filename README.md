# file_checklist
This application is for having a csv full of file paths. You can open each one easily, make a comment and check it off. Very good if you have a bunch of static data in files that needs to be manually changed and you want to do it all from one application. 

## Features

- Navigate through records using `j` (down) and `k` (up).
- Jump to a specific record by pressing `g`.
- Edit a record's comment and toggle its 'checked' status by pressing `e`.
- Toggle the 'checked' status on the fly using the spacebar.
- View help information by pressing `h`.
- Exit the application with `q`.

## Prerequisites

- .NET SDK
- Compatible with Windows environments that support .NET applications.

## Setup and Execution

1. Clone the repository or download the source code.
2. Navigate to the project directory in the terminal.
3. Run `dotnet build` to build the project.
4. Execute the application using `dotnet run`.
5. The application expects a CSV file named `ff.csv` in the project directory with the following format:
label_path, comment, checked
path_to_label1, comment1, true/false
path_to_label2, comment2, true/false
...
6. Follow the on-screen instructions to navigate and edit the records.

## Contributing

Feel free to fork the project and submit pull requests. You can also open issues for bugs found or feature requests.

## Authors

- Daniel Van Den Bosch


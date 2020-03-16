import sys
import os
import shutil


def valid_yes_no_response(prompt):
    print("Y / N\n")
    while(True):
        answer = input(prompt)
        if answer.capitalize() == 'Y':
            return True
        elif answer.capitalize() == 'N' :
            return False
        else:
            print('Unknown response. Please answer Y or N.\n')


def main(suffix, path):
    print(f'Looking for files ending in: {suffix} in directory : {path}\n')
    os.chdir(path)
    files = []
    for file in os.listdir():
        if file.endswith(suffix):
            files.append(file)

    print(f'Found {len(files)} files ending with {suffix}.')
    
    if  not valid_yes_no_response('Do you want to move these files to a new folder?\n'):
        print('Exit')
        return

    newFolder = path + '\\' + input('Enter new subfoler name:\n')
    os.mkdir(newFolder)

    for file in files:
        shutil.move(path + '\\' + file, newFolder+'\\'+file)
    

if __name__ == '__main__':
    main(sys.argv[1], sys.argv[2])
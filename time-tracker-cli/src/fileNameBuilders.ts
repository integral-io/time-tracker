import { Utilities } from './utilities';

export class FileNameBuilders {
    static getTimeEntryHistoryFileName(username: string): string {
        return `${Utilities.getHomeDirectoryPath()}${username}-hours.csv`;
    }
}
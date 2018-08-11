import { Utilities } from './utilities';

export class FileNameBuilders {
    static getTimeEntryHistoryFileName(username: String): String {
        return `${Utilities.getHomeDirectoryPath()}${username}-hours.json`;
    }
}
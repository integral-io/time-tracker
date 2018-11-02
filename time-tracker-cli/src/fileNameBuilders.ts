import { Utilities } from './utilities';

export class FileNameBuilders {
    static getTimeEntryHistoryFileName(username: string): string {
        return `${Utilities.getHomeDirectoryPath()}${username}-hours.csv`;
    }

    static getConfigFileName(): string {
      return `${Utilities.getHomeDirectoryPath()}itt_config.js`;
    }
}

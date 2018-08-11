export class Utilities {

    static getHomeDirectoryPath(): String {
        return `${process.env.HOME || process.env.HOMEPATH || process.env.USERPROFILE}/`;
    }
}
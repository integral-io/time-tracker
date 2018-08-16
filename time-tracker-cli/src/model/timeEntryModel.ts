export interface TimeEntryModel {
    username: string 
    hours: number
    project: string,
    date: string
}

export interface TimeEntryModel_A extends TimeEntryModel {
    logDate: string
}
import { Command, flags } from '@oclif/command'
import * as fs from 'fs'

import { TimeEntryModel } from '../model/timeEntryModel'

export default class Record extends Command {

  static username = "harry-plunger"
    
  static description = 'Record hours for a project. Defaults to 8 hours for today. '

  static examples = [
    `$ itt record <project> <hours> <YYYY-MM-DD>`,
  ]

  static flags = {
    help: flags.help(),
    date: flags.string({char: 'd', description: 'Override the date, format YYYY-MM-DD or MM-DD or DD', default: "today"})
  }

  static args = [
    {name: 'project', description: 'Project to record hours for', required: true},
    {name: 'hours', description: 'Hours to record, 7.5 is 7h and 30m', default: '8'},
  ]

  async run(): Promise<any> {
    const {args, flags} = this.parse(Record)
    const now = new Date();
    // todo: validate date is coming in correctlty, accept today or yesterday, or above examples

    const entry: TimeEntryModel = {
      project: args.project,
      hours: args.hours,
      username: Record.username,
      date: flags.date == "today" ? `${now.getFullYear()}-${now.getMonth()+1}-${now.getDate()}` : flags.date
    }
    // todo: separate method to save, append to array
    fs.appendFile(
      `${process.env.HOME || process.env.HOMEPATH || process.env.USERPROFILE}/${ Record.username }-hours.json`,
      `${JSON.stringify(entry)},\n`,
      ( error ) => {
        if( error ) console.error( error )
        console.log( 'File Updated' )
      })

    this.log(`${entry.username} Logged ${entry.hours} hours ${entry.date} for ${entry.project}`)

  }
}

import {Command, flags} from '@oclif/command'

export default class Report extends Command {
    
  static description = 'Builds report for recorded summary for a month or current year.'

  static examples = [
    `$ itt report <YYYY-MM>`,
  ]
  
  async run(): Promise<any> {
    const {args, flags} = this.parse(Report)
    throw new Error("Method not implemented.");
  }
}
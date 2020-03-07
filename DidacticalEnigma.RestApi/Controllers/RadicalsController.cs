﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DidacticalEnigma.Core.Models.LanguageService;
using DidacticalEnigma.RestApi.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace DidacticalEnigma.RestApi.Controllers
{
    [Route("radicals")]
    public class RadicalsController : Controller
    {
        [HttpGet("list")]
        [SwaggerOperation(OperationId = "ListRadicals")]
        public ActionResult<IEnumerable<string>> ListRadicals(
            [FromServices] IKanjiRadicalLookup radicalLookup)
        {
            return this.Ok(radicalLookup.AllRadicals.Select(r => r.ToString()));
        }

        [HttpGet("select")]
        [SwaggerOperation(OperationId = "SelectRadicals")]
        public ActionResult<KanjiLookupResult> SelectRadicals(
            [FromQuery(Name = "radical")] string commaDelimitedRadicals,
            [FromServices] IKanjiRadicalLookup radicalLookup)
        {
            commaDelimitedRadicals ??= "";
            var rawRadicals = commaDelimitedRadicals
                .Trim()
                .Split(",", StringSplitOptions.RemoveEmptyEntries);
            var radicals = rawRadicals
                .Select(r => CodePoint.FromString(r))
                .ToList();
            if (!radicals.Any())
            {
                return this.Ok(new KanjiLookupResult
                {
                    Kanji = Array.Empty<string>(),
                    PossibleRadicals = radicalLookup.AllRadicals
                        .Select(r => r.ToString())
                        .ToList()
                });
            }

            if (radicals.Except(radicalLookup.AllRadicals).Any())
            {
                return this.BadRequest();
            }

            var result = radicalLookup.SelectRadical(radicals);
            return this.Ok(new KanjiLookupResult
            {
                Kanji = result.Kanji
                    .Select(k => k.ToString())
                    .ToList(),
                PossibleRadicals = result.PossibleRadicals
                    .Where(r => r.Value)
                    .Select(r => r.Key.ToString())
                    .Concat(rawRadicals)
                    .ToList()
            });
        }
    }
}